using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.DTOs;
using SafeStockAPI.Models;

namespace SafeStockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimentacoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/movimentacoes?produtoId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimentacaoDTO>>> GetMovimentacoes(
            [FromQuery] int? produtoId = null)
        {
            var query = _context.Movimentacoes
                .Include(m => m.Produto)
                .AsQueryable();

            if (produtoId.HasValue)
                query = query.Where(m => m.ProdutoId == produtoId);

            return await query
                .OrderByDescending(m => m.Data)
                .Select(m => new MovimentacaoDTO
                {
                    Id = m.Id,
                    ProdutoId = m.ProdutoId,
                    ProdutoNome = m.Produto.Nome,
                    Quantidade = m.Quantidade,
                    Tipo = m.Tipo,
                    Data = m.Data
                })
                .ToListAsync();
        }

        // POST: api/movimentacoes
        [HttpPost]
        public async Task<ActionResult<Movimentacao>> PostMovimentacao(
            RegistrarMovimentacaoDTO dto)
        {
            var produto = await _context.Produtos.FindAsync(dto.ProdutoId);
            if (produto == null) return NotFound("Produto não encontrado");

            // Valida estoque para saída
            if (dto.Tipo == "SAIDA" && produto.Quantidade < dto.Quantidade)
                return BadRequest("Quantidade insuficiente em estoque");

            // Atualiza estoque
            produto.Quantidade += dto.Tipo == "ENTRADA" ? dto.Quantidade : -dto.Quantidade;

            var movimentacao = new Movimentacao
            {
                ProdutoId = dto.ProdutoId,
                Quantidade = dto.Quantidade,
                Tipo = dto.Tipo
            };

            _context.Movimentacoes.Add(movimentacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMovimentacao", new { id = movimentacao.Id }, movimentacao);
        }
    }
}
