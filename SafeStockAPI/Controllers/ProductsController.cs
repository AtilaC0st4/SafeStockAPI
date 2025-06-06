using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Models;
using SafeStockAPI.Services;
using System.Collections.Generic;
using System.Linq;

namespace SafeStockAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MLPriorityService _mlPriorityService;

        public ProductsController(AppDbContext context, MLPriorityService mlPriorityService)
        {
            _context = context;
            _mlPriorityService = mlPriorityService;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // GET: api/Products/5/prioridade-reposicao
        [HttpGet("{id}/prioridade-reposicao")]
        public IActionResult GetPrioridadeReposicao(int id)
        {
            var produto = _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Id == id);

            if (produto == null) return NotFound();

            var movimentacoes = _context.Movimentacoes
                .Where(m => m.ProdutoId == id)
                .ToList();

            var prioridade = _mlPriorityService.PredictPriority(produto, movimentacoes);

            return Ok(new
            {
                ProdutoId = id,
                Nome = produto.Nome,
                Prioridade = prioridade,
                EstoqueAtual = produto.Quantidade
            });
        }

        [HttpGet("ml-diagnostics")]
        public IActionResult GetMlDiagnostics()
        {
            var products = _context.Produtos
                .Include(p => p.Categoria)
                .Take(10)
                .ToList()
                .Select(p => new {
                    p.Id,
                    p.Nome,
                    p.Quantidade,
                    Prioridade = _mlPriorityService.PredictPriority(
                        p,
                        _context.Movimentacoes.Where(m => m.ProdutoId == p.Id).ToList())
                });

            return Ok(products);
        }
    }
}