namespace SafeStockAPI.DTOs
{
    public class DashboardDTO
    {
        public int TotalProdutos { get; set; }
        public int ProdutosEmEstoqueBaixo { get; set; }
        public int TotalCategorias { get; set; }
        public List<ProdutoStatusDTO> StatusProdutos { get; set; }
    }

    public class ProdutoStatusDTO
    {
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public int Quantidade { get; set; }
        public string Status { get; set; }
        public string CorStatus { get; set; } 
    }
}
