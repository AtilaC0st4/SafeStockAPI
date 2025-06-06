using Microsoft.ML;
using Microsoft.ML.Data;
using SafeStockAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace SafeStockAPI.Services
{
    public class MLPriorityService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private PredictionEngine<ProductAnalysisData, ProductPriorityPrediction> _predictionEngine;

        public MLPriorityService()
        {
            _mlContext = new MLContext(seed: 1);

            
            var initialData = new List<ProductAnalysisData>
            {
                // CRÍTICO (estoque zero)
                new() { CurrentStock=0, AvgMovement=5, DaysSinceLastMove=1, Priority="CRITICA" },
                
                // ALTA (1-4 itens)
                new() { CurrentStock=1, AvgMovement=8, DaysSinceLastMove=1, Priority="ALTA" },
                new() { CurrentStock=2, AvgMovement=6, DaysSinceLastMove=1, Priority="ALTA" },
                new() { CurrentStock=3, AvgMovement=7, DaysSinceLastMove=2, Priority="ALTA" },
                new() { CurrentStock=4, AvgMovement=5, DaysSinceLastMove=2, Priority="ALTA" },
                
                // MÉDIA (5-9 itens)
                new() { CurrentStock=5, AvgMovement=4, DaysSinceLastMove=3, Priority="MEDIA" },
                new() { CurrentStock=7, AvgMovement=3, DaysSinceLastMove=4, Priority="MEDIA" },
                new() { CurrentStock=9, AvgMovement=2, DaysSinceLastMove=5, Priority="MEDIA" },
                
                // BAIXA (10+ itens)
                new() { CurrentStock=10, AvgMovement=2, DaysSinceLastMove=7, Priority="BAIXA" },
                new() { CurrentStock=15, AvgMovement=1, DaysSinceLastMove=10, Priority="BAIXA" },
                new() { CurrentStock=20, AvgMovement=0.5f, DaysSinceLastMove=15, Priority="BAIXA" }
            };

            TrainModel(initialData);
            ValidateModel();
        }

        private void TrainModel(List<ProductAnalysisData> trainingData)
        {
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ProductAnalysisData.Priority))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(ProductAnalysisData.CurrentStock),
                    nameof(ProductAnalysisData.AvgMovement),
                    nameof(ProductAnalysisData.DaysSinceLastMove)))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            _model = pipeline.Fit(dataView);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductAnalysisData, ProductPriorityPrediction>(_model);
        }

        public string PredictPriority(Produto produto, List<Movimentacao> movimentacoes)
        {
            // Regra absoluta para estoque zero
            if (produto.Quantidade == 0) return "CRITICA";

            var relevantMovements = movimentacoes?.Where(m => m.Tipo == "SAIDA").ToList() ?? new List<Movimentacao>();
            var avgMovement = relevantMovements.Any() ?
                relevantMovements.Average(m => m.Quantidade) : 0;

            var input = new ProductAnalysisData
            {
                CurrentStock = produto.Quantidade,
                AvgMovement = (float)avgMovement,
                DaysSinceLastMove = relevantMovements.Any() ?
                    (float)(DateTime.Now - relevantMovements.Max(m => m.Data)).TotalDays : 30
            };

            var prediction = _predictionEngine.Predict(input);

            // Garantia adicional das faixas de estoque
            return produto.Quantidade switch
            {
                0 => "CRITICA",
                <= 4 => "ALTA",
                <= 9 => "MEDIA",
                _ => "BAIXA"
            };
        }

        private void ValidateModel()
        {
            var testCases = new (int quantidade, string expected)[]
            {
                (0, "CRITICA"),
                (1, "ALTA"), (2, "ALTA"), (3, "ALTA"), (4, "ALTA"),
                (5, "MEDIA"), (7, "MEDIA"), (9, "MEDIA"),
                (10, "BAIXA"), (15, "BAIXA"), (20, "BAIXA")
            };

            foreach (var testCase in testCases)
            {
                var testProduct = new Produto { Quantidade = testCase.quantidade };
                var result = PredictPriority(testProduct, new List<Movimentacao>());

                if (result != testCase.expected)
                {
                    Console.WriteLine($"ERRO: Para {testCase.quantidade} itens, esperado {testCase.expected} mas obtido {result}");
                }
            }
        }
    }

    public class ProductAnalysisData
    {
        public float CurrentStock { get; set; }
        public float AvgMovement { get; set; }
        public float DaysSinceLastMove { get; set; }
        public string Priority { get; set; }
    }

    public class ProductPriorityPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Priority { get; set; }
    }
}