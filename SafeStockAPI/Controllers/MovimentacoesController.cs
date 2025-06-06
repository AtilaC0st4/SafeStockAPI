using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.DTOs;
using SafeStockAPI.Models;

namespace SafeStockAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciar movimentações de estoque
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimentacoesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todas as movimentações ou filtra por produto
        /// </summary>
        /// <param name="produtoId">ID do produto para filtrar (opcional)</param>
        /// <returns>Lista de movimentações</returns>
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

        /// <summary>
        /// Cria uma nova movimentação de estoque
        /// </summary>
        /// <param name="dto">Dados da movimentação</param>
        /// <returns>A movimentação criada</returns>
        /// <response code="201">Retorna a movimentação criada</response>
        /// <response code="400">Se os dados forem inválidos ou estoque insuficiente</response>
        /// <response code="404">Se o produto não for encontrado</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Atualiza uma movimentação existente
        /// </summary>
        /// <param name="id">ID da movimentação</param>
        /// <param name="dto">Novos dados da movimentação</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Se a atualização for bem-sucedida</response>
        /// <response code="400">Se os dados forem inválidos ou estoque insuficiente</response>
        /// <response code="404">Se a movimentação ou produto não for encontrado</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutMovimentacao(int id, AtualizarMovimentacaoDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID da rota não corresponde ao ID da movimentação");
            }

            var movimentacao = await _context.Movimentacoes
                .Include(m => m.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimentacao == null)
            {
                return NotFound("Movimentação não encontrada");
            }

            var produto = movimentacao.Produto;
            var diferencaQuantidade = dto.Quantidade - movimentacao.Quantidade;

            // Validação para saída
            if (movimentacao.Tipo == "SAIDA" && produto.Quantidade < diferencaQuantidade)
            {
                return BadRequest("Ajuste inválido: estoque ficaria negativo");
            }

            // Reverte a movimentação original
            produto.Quantidade += movimentacao.Tipo == "ENTRADA" ? -movimentacao.Quantidade : movimentacao.Quantidade;

            // Aplica a nova movimentação
            produto.Quantidade += movimentacao.Tipo == "ENTRADA" ? dto.Quantidade : -dto.Quantidade;

            // Atualiza os dados da movimentação
            movimentacao.Quantidade = dto.Quantidade;
            movimentacao.Data = DateTime.UtcNow; // Atualiza a data para agora

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovimentacaoExists(id))
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
        /// Remove uma movimentação
        /// </summary>
        /// <param name="id">ID da movimentação a ser removida</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Se a remoção for bem-sucedida</response>
        /// <response code="404">Se a movimentação não for encontrada</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMovimentacao(int id)
        {
            var movimentacao = await _context.Movimentacoes
                .Include(m => m.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimentacao == null)
            {
                return NotFound();
            }

            var produto = movimentacao.Produto;

            // Reverte o estoque
            produto.Quantidade += movimentacao.Tipo == "ENTRADA" ? -movimentacao.Quantidade : movimentacao.Quantidade;

            _context.Movimentacoes.Remove(movimentacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovimentacaoExists(int id)
        {
            return _context.Movimentacoes.Any(e => e.Id == id);
        }
    }
}