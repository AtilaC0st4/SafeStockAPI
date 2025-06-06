using Microsoft.ML.Data;

namespace SafeStockAPI.Models
{
    public class ProductAnalysisData
    {
        [LoadColumn(0)] public float CurrentStock;       
        [LoadColumn(1)] public float AvgMovement;      
        [LoadColumn(2)] public float DaysSinceLastMove; 
        [LoadColumn(4)] public string Priority;         
    }
}
