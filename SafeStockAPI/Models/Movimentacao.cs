using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SafeStockAPI.Models
{
    [Table("MOVIMENTACOES")]
    public class Movimentacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Column("PRODUTO_ID")]
        public int ProdutoId { get; set; }

        [ForeignKey("ProdutoId")]
        public Produto Produto { get; set; }

        [Column("QUANTIDADE")]
        public int Quantidade { get; set; }

        [Required]
        [Column("TIPO", TypeName = "NVARCHAR2(10)")]
        public string Tipo { get; set; } 

        [Column("DATA")]
        public DateTime Data { get; set; } = DateTime.Now;
    }
}
