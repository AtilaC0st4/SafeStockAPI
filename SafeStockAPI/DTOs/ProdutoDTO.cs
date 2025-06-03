using System.ComponentModel.DataAnnotations;


public class ProdutoDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public int Quantidade { get; set; }
    public string Status { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; }
}

public class CriarProdutoDTO
{
    [Required(ErrorMessage = "Nome do produto é obrigatório")]
    [MaxLength(150, ErrorMessage = "Máximo de 150 caracteres")]
    public string Nome { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantidade não pode ser negativa")]
    public int QuantidadeInicial { get; set; } = 0;

    [Required(ErrorMessage = "Categoria é obrigatória")]
    public int CategoriaId { get; set; }
}