using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.DTOs;

namespace SafeStockAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDTO>> GetDashboard()
        {
            // Primeiro materializa os dados do banco
            var produtosCriticos = await _context.Produtos
                .Where(p => p.Quantidade <= 5)
                .Include(p => p.Categoria)
                .OrderBy(p => p.Quantidade)
                .Take(10)
                .Select(p => new
                {
                    p.Nome,
                    Categoria = p.Categoria.Nome,
                    p.Quantidade,
                    p.Status
                })
                .ToListAsync();

            // Agora mapeia para DTO (aqui podemos usar switch)
            var produtosDTO = produtosCriticos.Select(p => new ProdutoStatusDTO
            {
                Nome = p.Nome,
                Categoria = p.Categoria,
                Quantidade = p.Quantidade,
                Status = p.Status,
                CorStatus = p.Status switch // Switch agora é executado em memória
                {
                    "baixo" => "red",
                    "médio" => "yellow",
                    _ => "green"
                }
            }).ToList();

            return new DashboardDTO
            {
                TotalProdutos = await _context.Produtos.CountAsync(),
                ProdutosEmEstoqueBaixo = produtosDTO.Count(p => p.Status == "baixo"),
                TotalCategorias = await _context.Categorias.CountAsync(),
                StatusProdutos = produtosDTO
            };
        }
    }
}
