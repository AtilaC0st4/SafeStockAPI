using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.DTOs;
using SafeStockAPI.Models;

namespace SafeStockAPI.Controllers
{
    /// <summary>
    /// Controller para fornecer dados de dashboard e relatórios de estoque
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém dados consolidados para o dashboard de estoque
        /// </summary>
        /// <returns>
        /// Dados do dashboard incluindo:
        /// - Totais de produtos e categorias
        /// - Quantidade de produtos com estoque baixo
        /// - Lista completa de produtos com seus status de estoque
        /// </returns>
        /// <response code="200">Retorna os dados do dashboard</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<ActionResult<DashboardDTO>> GetDashboard()
        {
            // 1. Obter TODOS os produtos do banco de dados
            var todosProdutos = await _context.Produtos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            // 2. Mapear para ProdutoStatusDTO e determinar Status/CorStatus para CADA produto
            var produtosStatusDTO = todosProdutos.Select(p =>
            {
                string status;
                string corStatus;

                // Lógica para determinar o status e a cor com base na quantidade
                if (p.Quantidade <= 5) // Estoque Baixo
                {
                    status = "Baixo";
                    corStatus = "red";
                }
                else if (p.Quantidade <= 20) // Estoque Médio
                {
                    status = "Médio";
                    corStatus = "yellow";
                }
                else // Estoque Ideal/Alto
                {
                    status = "Ideal";
                    corStatus = "green";
                }

                return new ProdutoStatusDTO
                {
                    Nome = p.Nome,
                    Categoria = p.Categoria.Nome,
                    Quantidade = p.Quantidade,
                    Status = status,
                    CorStatus = corStatus
                };
            }).ToList();

            // 3. Calcular os totais para o DashboardDTO
            int totalProdutos = await _context.Produtos.CountAsync();
            int produtosEmEstoqueBaixo = produtosStatusDTO.Count(p => p.Status == "Baixo");
            int totalCategorias = await _context.Categorias.CountAsync();

            // Retorna o DashboardDTO completo
            return new DashboardDTO
            {
                TotalProdutos = totalProdutos,
                ProdutosEmEstoqueBaixo = produtosEmEstoqueBaixo,
                TotalCategorias = totalCategorias,
                StatusProdutos = produtosStatusDTO
            };
        }
    }
}