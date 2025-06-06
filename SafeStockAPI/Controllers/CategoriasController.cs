using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Models;
using SafeStockAPI.DTOs;

namespace SafeStockAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de categorias de produtos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todas as categorias cadastradas
        /// </summary>
        /// <returns>Lista de categorias com contagem de produtos</returns>
        /// <response code="200">Retorna a lista de categorias</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Include(c => c.Produtos)
                .Select(c => new CategoriaDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    TotalProdutos = c.Produtos.Count
                })
                .ToListAsync();

            return Ok(categorias); // <- aqui está o fix
        }

        /// <summary>
        /// Obtém uma categoria específica pelo ID
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <returns>Dados da categoria com contagem de produtos</returns>
        /// <response code="200">Retorna a categoria solicitada</response>
        /// <response code="404">Se a categoria não for encontrada</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoriaDTO>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Produtos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(new CategoriaDTO
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                TotalProdutos = categoria.Produtos.Count
            });

        }

        /// <summary>
        /// Cria uma nova categoria
        /// </summary>
        /// <param name="dto">Dados da nova categoria</param>
        /// <returns>A categoria recém-criada</returns>
        /// <response code="201">Retorna a categoria criada</response>
        /// <response code="400">Se os dados forem inválidos</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Categoria>> PostCategoria(CriarCategoriaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoria = new Categoria { Nome = dto.Nome };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = categoria.Id },
                new CategoriaDTO
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    TotalProdutos = 0
                });
        }

        /// <summary>
        /// Atualiza uma categoria existente
        /// </summary>
        /// <param name="id">ID da categoria a ser atualizada</param>
        /// <param name="dto">Novos dados da categoria</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Atualização bem-sucedida</response>
        /// <response code="400">Se os IDs não coincidirem ou dados forem inválidos</response>
        /// <response code="404">Se a categoria não for encontrada</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCategoria(int id, AtualizarCategoriaDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID da rota não corresponde ao ID do objeto");
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Nome = dto.Nome;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Exclui uma categoria
        /// </summary>
        /// <param name="id">ID da categoria a ser excluída</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Exclusão bem-sucedida</response>
        /// <response code="400">Se a categoria tiver produtos vinculados</response>
        /// <response code="404">Se a categoria não for encontrada</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}