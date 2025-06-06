using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeStockAPI.Controllers;
using SafeStockAPI.DTOs;
using SafeStockAPI.Models;
using Xunit;

namespace SafeStockAPI.Tests.Controllers
{
    public class CategoriasControllerTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly CategoriasController _controller;

        public CategoriasControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();
            _controller = new CategoriasController(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetCategorias_ReturnsEmptyList_WhenNoCategories()
        {
            // Act
            var result = await _controller.GetCategorias();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<CategoriaDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var categorias = Assert.IsType<List<CategoriaDTO>>(okResult.Value);
            Assert.Empty(categorias);
        }

        [Fact]
        public async Task GetCategorias_ReturnsCategories_WhenCategoriesExist()
        {
            // Arrange
            _dbContext.Categorias.Add(new Categoria { Nome = "Teste 1" });
            _dbContext.Categorias.Add(new Categoria { Nome = "Teste 2" });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetCategorias();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<CategoriaDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var categorias = Assert.IsType<List<CategoriaDTO>>(okResult.Value);
            Assert.Equal(2, categorias.Count);
        }

        [Fact]
        public async Task GetCategoria_ReturnsNotFound_WhenIdNotExists()
        {
            // Act
            var result = await _controller.GetCategoria(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCategoria_ReturnsCategory_WhenIdExists()
        {
            // Arrange
            var categoria = new Categoria { Nome = "Teste" };
            _dbContext.Categorias.Add(categoria);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetCategoria(categoria.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<CategoriaDTO>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var categoriaDto = Assert.IsType<CategoriaDTO>(okResult.Value);
            Assert.Equal(categoria.Nome, categoriaDto.Nome);
        }

        [Fact]
        public async Task PostCategoria_CreatesNewCategory()
        {
            // Arrange
            var dto = new CriarCategoriaDTO { Nome = "Nova Categoria" };

            // Act
            var result = await _controller.PostCategoria(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var categoriaDto = Assert.IsType<CategoriaDTO>(createdAtActionResult.Value);
            Assert.Equal(dto.Nome, categoriaDto.Nome);
            Assert.True(categoriaDto.Id > 0);
        }

        [Fact]
        public async Task PostCategoria_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Nome", "Required");
            var dto = new CriarCategoriaDTO { Nome = null! };

            // Act
            var result = await _controller.PostCategoria(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PutCategoria_UpdatesExistingCategory()
        {
            // Arrange
            var categoria = new Categoria { Nome = "Original" };
            _dbContext.Categorias.Add(categoria);
            await _dbContext.SaveChangesAsync();

            var dto = new AtualizarCategoriaDTO { Id = categoria.Id, Nome = "Atualizado" };

            // Act
            var result = await _controller.PutCategoria(categoria.Id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify update
            var updatedCategoria = await _dbContext.Categorias.FindAsync(categoria.Id);
            Assert.Equal(dto.Nome, updatedCategoria?.Nome);
        }

        [Fact]
        public async Task PutCategoria_ReturnsBadRequest_WhenIdsDontMatch()
        {
            // Arrange
            var dto = new AtualizarCategoriaDTO { Id = 1, Nome = "Teste" };

            // Act
            var result = await _controller.PutCategoria(2, dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCategoria_RemovesCategory_WhenNoProducts()
        {
            // Arrange
            var categoria = new Categoria { Nome = "Para Excluir" };
            _dbContext.Categorias.Add(categoria);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCategoria(categoria.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.False(await _dbContext.Categorias.AnyAsync(c => c.Id == categoria.Id));
        }

        [Fact]
        public async Task DeleteCategoria_ReturnsBadRequest_WhenCategoryHasProducts()
        {
            // Arrange
            var categoria = new Categoria { Nome = "Com Produtos" };
            var produto = new Produto
            {
                Nome = "Produto Teste",
                Quantidade = 15, // Define o status como "ideal"
                Categoria = categoria
            };

            _dbContext.Categorias.Add(categoria);
            _dbContext.Produtos.Add(produto);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCategoria(categoria.Id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Não é possível excluir categorias com produtos vinculados", badRequestResult.Value);
        }
    }
}