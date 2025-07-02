using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Linq;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.ViewModels;
using TotemPWA.Models.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http; // Necessário para usar Request.Headers

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> List(string? searchTerm)
        {
            var allProducts = await _context.Products.Include(p => p.Category).ToListAsync();

            IEnumerable<Product> products = allProducts;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => (p.Name != null && p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                                                (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            ViewData["CurrentSearchTerm"] = searchTerm;

            // ***** AQUI ESTÁ A LÓGICA CHAVE: *****
            // Se a requisição for AJAX (feita pelo JavaScript), retorna os dados em formato JSON.
            // O JavaScript no frontend vai construir o HTML a partir desse JSON.
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(products.ToList());
            }

            // Se não for uma requisição AJAX (primeira carga da página),
            // retorna a View completa, que já tem o loop @foreach para renderizar os produtos.
            return View(products.ToList());
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var allIngredients = await _context.Ingredients.ToListAsync();
            var viewModel = new ProductViewModel
            {
                Product = new Product { Name = string.Empty, Description = string.Empty, Price = 0.01M, Active = true },
                Categories = await GetCategorySelectListAsync(),
                SelectedIngredients = allIngredients.ToDictionary(
                    i => i.Id,
                    i => new IngredientSelectionViewModel
                    {
                        IngredientId = i.Id,
                        IngredientName = i.Name,
                        IsSelected = false,
                        Price = i.Price,
                        Limit = i.Limit,
                        Quantity = 0 
                    }
                )
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel viewModel)
        {
            ModelState.Remove("Product.ImageUrl");

            if (ModelState.IsValid)
            {
                // ... (lógica de upload de imagem existente) ...

                _context.Products.Add(viewModel.Product);
                await _context.SaveChangesAsync(); // Salva o produto primeiro para obter o Id

                // Salva as associações de ingredientes
                if (viewModel.SelectedIngredients != null)
                {
                    await SaveProductAdditionals(viewModel.Product.Id, viewModel.SelectedIngredients);
                }

                TempData["Message"] = "Produto criado com sucesso!";
                return RedirectToAction("List");
            }

            // Se o modelo for inválido, repopule as categorias e ingredientes
            viewModel.Categories = await GetCategorySelectListAsync();
            var allIngredients = await _context.Ingredients.ToListAsync();
            viewModel.SelectedIngredients ??= new Dictionary<int, IngredientSelectionViewModel>();
            foreach (var ingredient in allIngredients)
            {
                if (!viewModel.SelectedIngredients.ContainsKey(ingredient.Id))
                {
                    viewModel.SelectedIngredients.Add(ingredient.Id, new IngredientSelectionViewModel
                    {
                        IngredientId = ingredient.Id,
                        IngredientName = ingredient.Name,
                        IsSelected = false,
                        Price = ingredient.Price,
                        Limit = ingredient.Limit,
                        Quantity = 0 // Inicializa a quantidade na repopulação
                    });
                }
            }
            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Additionals!)
                    .ThenInclude(a => a.Ingredient)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var allIngredients = await _context.Ingredients.ToListAsync();

            var selectedIngredientsDict = allIngredients.ToDictionary(
                i => i.Id,
                i => {
                    var existingAdditional = product.Additionals?.FirstOrDefault(pa => pa.IngredientId == i.Id);
                    return new IngredientSelectionViewModel
                    {
                        IngredientId = i.Id,
                        IngredientName = i.Name,
                        IsSelected = existingAdditional != null, // Continua indicando se está associado
                        Price = i.Price,
                        Limit = i.Limit,
                        Quantity = existingAdditional?.Quantity ?? 0 
                    };
                }
            );

            var viewModel = new ProductViewModel
            {
                Product = product,
                Categories = await GetCategorySelectListAsync(),
                SelectedIngredients = selectedIngredientsDict
            };

            return View(viewModel);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel viewModel)
        {
            if (viewModel.Product.Id == 0) return BadRequest("ID do produto não fornecido.");

            ModelState.Remove("Product.ImageUrl");

            if (ModelState.IsValid)
            {
                var productInDb = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == viewModel.Product.Id);
                if (productInDb == null) return NotFound();

                // ... (lógica de upload/manutenção de imagem existente) ...

                try
                {
                    _context.Products.Update(viewModel.Product);
                    await _context.SaveChangesAsync();

                    // Salva as associações de ingredientes
                    if (viewModel.SelectedIngredients != null)
                    {
                        await SaveProductAdditionals(viewModel.Product.Id, viewModel.SelectedIngredients);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == viewModel.Product.Id)) return NotFound();
                    throw;
                }
                TempData["Message"] = "Produto atualizado com sucesso!";
                return RedirectToAction("List");
            }

            // Se o modelo for inválido, repopule as categorias e ingredientes
            viewModel.Categories = await GetCategorySelectListAsync();
            var allIngredients = await _context.Ingredients.ToListAsync();
           viewModel.SelectedIngredients ??= new Dictionary<int, IngredientSelectionViewModel>();
            foreach (var ingredient in allIngredients)
            {
                if (!viewModel.SelectedIngredients.ContainsKey(ingredient.Id))
                {
                    viewModel.SelectedIngredients.Add(ingredient.Id, new IngredientSelectionViewModel
                    {
                        IngredientId = ingredient.Id,
                        IngredientName = ingredient.Name,
                        IsSelected = false,
                        Price = ingredient.Price,
                        Limit = ingredient.Limit,
                        Quantity = 0 // Inicializa a quantidade na repopulação
                    });
                }
            }
            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost("{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default_product.png"))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Produto excluído com sucesso!";
            }
            return RedirectToAction("List");
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();
        }
        
        private async Task SaveProductAdditionals(int productId, Dictionary<int, IngredientSelectionViewModel> selectedIngredients)
        {
            var existingAdditionals = await _context.Additionals
                                                    .Where(a => a.ProductId == productId)
                                                    .ToListAsync();
            _context.Additionals.RemoveRange(existingAdditionals);
            await _context.SaveChangesAsync();

            foreach (var entry in selectedIngredients.Where(s => s.Value.IsSelected))
            {
                _context.Additionals.Add(new Additional
                {
                    ProductId = productId,
                    IngredientId = entry.Key,
                    Quantity = entry.Value.Quantity 
                   
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}