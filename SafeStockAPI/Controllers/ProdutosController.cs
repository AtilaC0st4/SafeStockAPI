using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Models;
using SafeStockAPI.DTOs;
using System;

namespace SafeStockAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciar produtos e suas quantidades em estoque
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os produtos cadastrados
        /// </summary>
        /// <returns>Lista de produtos com informações de categoria</returns>
        /// <response code="200">Retorna a lista de produtos</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

        /// <summary>
        /// Obtém um produto específico pelo ID
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Dados do produto solicitado</returns>
        /// <response code="200">Retorna o produto encontrado</response>
        /// <response code="404">Se o produto não for encontrado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Obtém produtos filtrados por categoria
        /// </summary>
        /// <param name="categoriaId">ID da categoria para filtro</param>
        /// <returns>Lista de produtos da categoria especificada</returns>
        /// <response code="200">Retorna a lista de produtos da categoria</response>
        [HttpGet("por-categoria/{categoriaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosPorCategoria(int categoriaId)
        {
            return await _context.Produtos
                .Where(p => p.CategoriaId == categoriaId)
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

        /// <summary>
        /// Cria um novo produto no sistema
        /// </summary>
        /// <param name="dto">Dados do novo produto</param>
        /// <returns>O produto criado</returns>
        /// <response code="201">Retorna o produto criado</response>
        /// <response code="400">Se os dados do produto forem inválidos</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                    Tipo = "ENTRADA",
                    Data = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            var categoriaNome = _context.Categorias
                                    .FirstOrDefault(c => c.Id == produto.CategoriaId)?.Nome;

            return CreatedAtAction(
                nameof(GetProduto),
                new { id = produto.Id },
                new ProdutoDTO
                {
                    Id = produto.Id,
                    Nome = produto.Nome,
                    Quantidade = produto.Quantidade,
                    Status = produto.Status,
                    CategoriaId = produto.CategoriaId,
                    CategoriaNome = categoriaNome
                });
        }

        /// <summary>
        /// Adiciona quantidade ao estoque de um produto
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <param name="quantidade">Quantidade a ser adicionada</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Operação realizada com sucesso</response>
        /// <response code="400">Se a quantidade for inválida</response>
        /// <response code="404">Se o produto não for encontrado</response>
        [HttpPut("{id}/adicionar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdicionarEstoque(int id, [FromBody] int quantidade)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound("Produto não encontrado.");

            if (quantidade <= 0)
            {
                return BadRequest("A quantidade a adicionar deve ser maior que zero.");
            }

            produto.Quantidade += quantidade;

            _context.Movimentacoes.Add(new Movimentacao
            {
                ProdutoId = id,
                Quantidade = quantidade,
                Tipo = "ENTRADA",
                Data = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Retira quantidade do estoque de um produto
        /// </summary>
        /// <param name="dto">Dados da retirada (ID do produto e quantidade)</param>
        /// <returns>Resultado da operação</returns>
        /// <response code="200">Retirada realizada com sucesso</response>
        /// <response code="400">Se a quantidade for inválida ou insuficiente</response>
        /// <response code="404">Se o produto não for encontrado</response>
        [HttpPost("retirar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RetirarEstoque([FromBody] RetiradaProdutoDTO dto)
        {
            var produto = await _context.Produtos.FindAsync(dto.ProdutoId);
            if (produto == null)
            {
                return NotFound(new { message = "Produto não encontrado." });
            }

            if (dto.Quantidade <= 0)
            {
                return BadRequest(new { message = "A quantidade para retirada deve ser maior que zero." });
            }

            if (dto.Quantidade > produto.Quantidade)
            {
                return BadRequest(new { message = $"Quantidade a retirar ({dto.Quantidade}) é superior à disponível ({produto.Quantidade})." });
            }

            produto.Quantidade -= dto.Quantidade;

            _context.Movimentacoes.Add(new Movimentacao
            {
                ProdutoId = dto.ProdutoId,
                Quantidade = dto.Quantidade,
                Tipo = "SAÍDA",
                Data = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Retirada de {dto.Quantidade} unidade(s) de {produto.Nome} registrada com sucesso!" });
        }

        /// <summary>
        /// Remove um produto do sistema
        /// </summary>
        /// <param name="id">ID do produto a ser removido</param>
        /// <returns>Nenhum conteúdo</returns>
        /// <response code="204">Produto removido com sucesso</response>
        /// <response code="404">Se o produto não for encontrado</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            var movimentacoes = await _context.Movimentacoes
                .Where(m => m.ProdutoId == id)
                .ToListAsync();

            _context.Movimentacoes.RemoveRange(movimentacoes);
            _context.Produtos.Remove(produto);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}