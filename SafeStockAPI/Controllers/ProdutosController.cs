using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Models;
using SafeStockAPI.DTOs;

namespace SafeStockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutos()
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Select(p => new ProdutoDTO
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Quantidade = p.Quantidade,
                    Status = p.Status,
                    CategoriaId = p.CategoriaId,
                    CategoriaNome = p.Categoria.Nome
                })
                .ToListAsync();
        }

        // GET: api/produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDTO>> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound();
            }

            return new ProdutoDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Quantidade = produto.Quantidade,
                Status = produto.Status,
                CategoriaId = produto.CategoriaId,
                CategoriaNome = produto.Categoria?.Nome
            };
        }

        // POST: api/produtos
        [HttpPost]
        public async Task<ActionResult<ProdutoDTO>> PostProduto(CriarProdutoDTO dto)
        {
            var produto = new Produto
            {
                Nome = dto.Nome,
                Quantidade = dto.QuantidadeInicial,
                CategoriaId = dto.CategoriaId
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            if (dto.QuantidadeInicial > 0)
            {
                _context.Movimentacoes.Add(new Movimentacao
                {
                    ProdutoId = produto.Id,
                    Quantidade = dto.QuantidadeInicial,
                    Tipo = "ENTRADA"
                });
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(
                nameof(GetProduto), // Referência segura ao método
                new { id = produto.Id },
                new ProdutoDTO
                {
                    Id = produto.Id,
                    Nome = produto.Nome,
                    Quantidade = produto.Quantidade,
                    Status = produto.Status,
                    CategoriaId = produto.CategoriaId,
                    CategoriaNome = _context.Categorias
                        .FirstOrDefault(c => c.Id == produto.CategoriaId)?.Nome
                });
        }

        // PUT: api/produtos/5/adicionar
        [HttpPut("{id}/adicionar")]
        public async Task<IActionResult> AdicionarEstoque(int id, [FromBody] int quantidade)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            produto.Quantidade += quantidade;

            _context.Movimentacoes.Add(new Movimentacao
            {
                ProdutoId = id,
                Quantidade = quantidade,
                Tipo = "ENTRADA"
            });

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}