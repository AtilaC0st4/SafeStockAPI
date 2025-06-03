using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações específicas para Oracle
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("PRODUTOS");
            entity.Property(p => p.Id)
                .HasColumnName("ID")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd();

            entity.Property(p => p.Nome)
                .HasColumnName("NOME")
                .HasColumnType("NVARCHAR2(100)")
                .IsRequired();

            entity.Property(p => p.Quantidade)
                .HasColumnName("QUANTIDADE")
                .HasColumnType("NUMBER(10)")
                .HasDefaultValue(0);

            entity.Property(p => p.CategoriaId)
                .HasColumnName("CATEGORIA_ID")
                .HasColumnType("NUMBER(10)");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("CATEGORIAS");
            entity.Property(c => c.Id)
                .HasColumnName("ID")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd();

            entity.Property(c => c.Nome)
                .HasColumnName("NOME")
                .HasColumnType("NVARCHAR2(50)")
                .IsRequired();
        });

        modelBuilder.Entity<Movimentacao>(entity =>
        {
            entity.ToTable("MOVIMENTACOES");
            entity.Property(m => m.Id)
                .HasColumnName("ID")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd();

            entity.Property(m => m.ProdutoId)
                .HasColumnName("PRODUTO_ID")
                .HasColumnType("NUMBER(10)");

            entity.Property(m => m.Quantidade)
                .HasColumnName("QUANTIDADE")
                .HasColumnType("NUMBER(10)");

            entity.Property(m => m.Tipo)
                .HasColumnName("TIPO")
                .HasColumnType("NVARCHAR2(10)");

            entity.Property(m => m.Data)
                .HasColumnName("DATA")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("SYSTIMESTAMP");
        });
    }
}