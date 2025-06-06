using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SafeStockAPI.Models
{
    [Table("CATEGORIAS")]
    public class Categoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("NOME", TypeName = "NVARCHAR2(100)")]
        public string Nome { get; set; }

        
        public ICollection<Produto> Produtos { get; set; }
    }
}
