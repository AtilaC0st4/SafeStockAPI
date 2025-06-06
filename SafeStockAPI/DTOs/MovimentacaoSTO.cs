using System.ComponentModel.DataAnnotations;

namespace SafeStockAPI.DTOs
{
  

    public class MovimentacaoDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public string Tipo { get; set; } 
        public DateTime Data { get; set; }
    }

    public class AtualizarMovimentacaoDTO
    {
        public int Id { get; set; }
        public int Quantidade { get; set; }
    }

    public class RegistrarMovimentacaoDTO
    {
        [Required(ErrorMessage = "ID do produto é obrigatório")]
        public int ProdutoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser positiva")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "Tipo de movimentação é obrigatório")]
        [RegularExpression("^(ENTRADA|SAIDA)$", ErrorMessage = "Tipo deve ser ENTRADA ou SAIDA")]
        public string Tipo { get; set; }
    }
}
