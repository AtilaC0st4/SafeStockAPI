using System.ComponentModel.DataAnnotations;


public class CategoriaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public int TotalProdutos { get; set; } // Contagem de produtos na categoria
}

public class CriarCategoriaDTO
{
    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [MaxLength(100, ErrorMessage = "Máximo de 100 caracteres")]
    public string Nome { get; set; }
}

public class AtualizarCategoriaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
}