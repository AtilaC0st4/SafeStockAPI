using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SafeStockAPI.Models
{
    [Table("PRODUTOS")]
    public class Produto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("NOME", TypeName = "NVARCHAR2(150)")]
        public string Nome { get; set; }

        [Column("QUANTIDADE")]
        public int Quantidade { get; set; } = 0;

        [NotMapped] // Não persiste no banco (é calculado)
        public string Status => Quantidade switch
        {
            > 10 => "ideal",
            > 5 => "médio",
            _ => "baixo"
        };

        // Chave estrangeira
        [Column("CATEGORIA_ID")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }
    }
}
