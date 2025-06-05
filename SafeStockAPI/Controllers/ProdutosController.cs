using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Models;
using SafeStockAPI.DTOs;
using System; // Necessário para DateTime.UtcNow

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

        // --- Endpoints de Leitura (GET) ---

        // GET: api/produtos
        /// <summary>
        /// Obtém uma lista de todos os produtos, incluindo suas categorias.
        /// </summary>
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
                    CategoriaNome = p.Categoria.Nome // Garante que o nome da categoria seja incluído
                })
                .ToListAsync();
        }

        // GET: api/produtos/5
        /// <summary>
        /// Obtém um produto específico pelo seu ID, incluindo a categoria.
        /// </summary>
        /// <param name="id">ID do produto.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDTO>> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound(); // Retorna 404 se o produto não for encontrado
            }

            return new ProdutoDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Quantidade = produto.Quantidade,
                Status = produto.Status,
                CategoriaId = produto.CategoriaId,
                CategoriaNome = produto.Categoria?.Nome // Uso de operador ? para segurança caso a categoria seja nula
            };
        }

        // GET: api/produtos/por-categoria/5
        /// <summary>
        /// Obtém uma lista de produtos filtrados por categoria.
        /// </summary>
        /// <param name="categoriaId">ID da categoria.</param>
        [HttpGet("por-categoria/{categoriaId}")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosPorCategoria(int categoriaId)
        {
            return await _context.Produtos
                .Where(p => p.CategoriaId == categoriaId) // Filtra pela CategoriaId
                .Include(p => p.Categoria) // Inclui a categoria para popular CategoriaNome
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

        // --- Endpoints de Escrita/Modificação ---

        // POST: api/produtos
        /// <summary>
        /// Cria um novo produto e registra uma movimentação de entrada se a quantidade inicial for maior que zero.
        /// </summary>
        /// <param name="dto">Dados para criação do produto.</param>
        [HttpPost]
        public async Task<ActionResult<ProdutoDTO>> PostProduto(CriarProdutoDTO dto)
        {
            // Cria o novo produto com os dados do DTO
            var produto = new Produto
            {
                Nome = dto.Nome,
                Quantidade = dto.QuantidadeInicial,
                CategoriaId = dto.CategoriaId
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync(); // Salva o produto para obter o ID

            // Se houver quantidade inicial, registra uma movimentação de ENTRADA
            if (dto.QuantidadeInicial > 0)
            {
                _context.Movimentacoes.Add(new Movimentacao
                {
                    ProdutoId = produto.Id,
                    Quantidade = dto.QuantidadeInicial,
                    Tipo = "ENTRADA",
                    Data = DateTime.UtcNow // <-- CORRIGIDO: Usando 'Data'
                });
                await _context.SaveChangesAsync(); // Salva a movimentação
            }

            // Busca o nome da categoria para o DTO de retorno
            var categoriaNome = _context.Categorias
                                    .FirstOrDefault(c => c.Id == produto.CategoriaId)?.Nome;

            // Retorna a representação do produto criado no formato DTO
            return CreatedAtAction(
                nameof(GetProduto), // Referência segura ao método GET para o novo produto
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

        // PUT: api/produtos/5/adicionar
        /// <summary>
        /// Adiciona uma quantidade ao estoque de um produto existente e registra uma movimentação de entrada.
        /// </summary>
        /// <param name="id">ID do produto.</param>
        /// <param name="quantidade">Quantidade a ser adicionada.</param>
        [HttpPut("{id}/adicionar")]
        public async Task<IActionResult> AdicionarEstoque(int id, [FromBody] int quantidade)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound("Produto não encontrado."); // Mensagem mais específica

            if (quantidade <= 0)
            {
                return BadRequest("A quantidade a adicionar deve ser maior que zero.");
            }

            produto.Quantidade += quantidade; // Atualiza a quantidade do produto

            // Registra a movimentação de ENTRADA
            _context.Movimentacoes.Add(new Movimentacao
            {
                ProdutoId = id,
                Quantidade = quantidade,
                Tipo = "ENTRADA",
                Data = DateTime.UtcNow // <-- CORRIGIDO: Usando 'Data'
            });

            await _context.SaveChangesAsync(); // Salva as alterações e a nova movimentação

            return NoContent(); // Retorna 204 No Content para sucesso sem corpo de resposta
        }

        // POST: api/produtos/retirar
        /// <summary>
        /// Retira uma quantidade do estoque de um produto existente e registra uma movimentação de saída.
        /// </summary>
        /// <param name="dto">Dados da retirada (ID do produto e quantidade).</param>
        [HttpPost("retirar")]
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

            // Validação crucial: verifica se há estoque suficiente
            if (dto.Quantidade > produto.Quantidade)
            {
                return BadRequest(new { message = $"Quantidade a retirar ({dto.Quantidade}) é superior à disponível ({produto.Quantidade})." });
            }

            produto.Quantidade -= dto.Quantidade; // Decrementa a quantidade do produto

            // Registra a movimentação de SAÍDA
            _context.Movimentacoes.Add(new Movimentacao
            {
                ProdutoId = dto.ProdutoId,
                Quantidade = dto.Quantidade,
                Tipo = "SAÍDA", // Define o tipo de movimentação como SAÍDA
                Data = DateTime.UtcNow // <-- CORRIGIDO: Usando 'Data'
            });

            await _context.SaveChangesAsync(); // Salva as mudanças no banco de dados

            // Retorna uma resposta de sucesso
            return Ok(new { message = $"Retirada de {dto.Quantidade} unidade(s) de {produto.Nome} registrada com sucesso!" });
        }


        // DELETE: api/produtos/5
        /// <summary>
        /// Exclui um produto e todas as suas movimentações de estoque associadas.
        /// </summary>
        /// <param name="id">ID do produto a ser excluído.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            // Remove todas as movimentações relacionadas ao produto ANTES de remover o produto
            var movimentacoes = await _context.Movimentacoes
                .Where(m => m.ProdutoId == id)
                .ToListAsync();

            _context.Movimentacoes.RemoveRange(movimentacoes);

            // Agora remove o produto
            _context.Produtos.Remove(produto);

            await _context.SaveChangesAsync(); // Salva as exclusões

            return NoContent(); // Retorna 204 No Content para sucesso sem corpo de resposta
        }
    }
}