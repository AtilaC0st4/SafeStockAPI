using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.DTOs; // Certifique-se de que seus DTOs estão neste namespace
using SafeStockAPI.Models; // Certifique-se de que seu modelo Produto está neste namespace

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
            // 1. Obter TODOS os produtos do banco de dados
            // Removendo a cláusula .Where(p => p.Quantidade <= 5)
            var todosProdutos = await _context.Produtos
                .Include(p => p.Categoria) // Inclui a categoria para acesso ao nome
                .OrderBy(p => p.Nome) // Ordena os produtos para uma lista consistente
                .ToListAsync();

            // 2. Mapear para ProdutoStatusDTO e determinar Status/CorStatus para CADA produto
            var produtosStatusDTO = todosProdutos.Select(p =>
            {
                string status;
                string corStatus;

                // Lógica para determinar o status e a cor com base na quantidade
                // Você pode ajustar esses limiares conforme a necessidade do seu negócio
                if (p.Quantidade <= 5) // Ex: Estoque Baixo
                {
                    status = "Baixo";
                    corStatus = "red";
                }
                else if (p.Quantidade <= 20) // Ex: Estoque Médio
                {
                    status = "Médio";
                    corStatus = "yellow";
                }
                else // Ex: Estoque Ideal/Alto
                {
                    status = "Ideal";
                    corStatus = "green";
                }

                return new ProdutoStatusDTO
                {
                    Nome = p.Nome,
                    Categoria = p.Categoria.Nome, // Pega o nome da categoria
                    Quantidade = p.Quantidade,
                    Status = status,
                    CorStatus = corStatus
                };
            }).ToList();

            // 3. Calcular os totais para o DashboardDTO
            int totalProdutos = await _context.Produtos.CountAsync();
            int produtosEmEstoqueBaixo = produtosStatusDTO.Count(p => p.Status == "Baixo"); // Conta os que foram definidos como 'Baixo'
            int totalCategorias = await _context.Categorias.CountAsync();

            // Retorna o DashboardDTO completo
            return new DashboardDTO
            {
                TotalProdutos = totalProdutos,
                ProdutosEmEstoqueBaixo = produtosEmEstoqueBaixo,
                TotalCategorias = totalCategorias,
                StatusProdutos = produtosStatusDTO // Agora contém TODOS os produtos
            };
        }
    }
}