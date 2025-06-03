using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Models;
using SafeStockAPI.DTOs;

namespace SafeStockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategorias()
        {
            return await _context.Categorias
                .Select(c => new CategoriaDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    TotalProdutos = c.Produtos.Count
                })
                .ToListAsync();
        }

        // GET: api/categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDTO>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Produtos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return new CategoriaDTO
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                TotalProdutos = categoria.Produtos.Count
            };
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(CriarCategoriaDTO dto)
        {
            var categoria = new Categoria { Nome = dto.Nome };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoria), // Usando nameof para evitar erros de digitação
                new { id = categoria.Id },
                new CategoriaDTO
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    TotalProdutos = 0
                });
        }

        // DELETE: api/categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Produtos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
                return NotFound();

            if (categoria.Produtos.Any())
                return BadRequest("Não é possível excluir categorias com produtos vinculados");

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}