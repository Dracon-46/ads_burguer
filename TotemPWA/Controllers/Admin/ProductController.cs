using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Linq;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.ViewModels;
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
            var viewModel = new ProductViewModel
            {
                Product = new Product { Name = string.Empty, Description = string.Empty, Price = 0.01M, Active = true },
                Categories = await GetCategorySelectListAsync()
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
                if (viewModel.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ImageFile.CopyToAsync(fileStream);
                    }
                    viewModel.Product.ImageUrl = "/images/products/" + uniqueFileName;
                }
                else
                {
                    viewModel.Product.ImageUrl = "/images/products/default_product.png";
                }

                _context.Products.Add(viewModel.Product);
                await _context.SaveChangesAsync();
                
                TempData["Message"] = "Produto criado com sucesso!";
                return RedirectToAction("List");
            }

            viewModel.Categories = await GetCategorySelectListAsync();
            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var viewModel = new ProductViewModel
            {
                Product = product,
                Categories = await GetCategorySelectListAsync()
            };

            return View(viewModel);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel viewModel)
        {
            if (viewModel.Product.Id == 0)
            {
                return BadRequest("ID do produto não fornecido.");
            }

            ModelState.Remove("Product.ImageUrl"); 

            if (ModelState.IsValid)
            {
                var productInDb = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == viewModel.Product.Id);
                if (productInDb == null) return NotFound();

                if (viewModel.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(productInDb.ImageUrl) && !productInDb.ImageUrl.Contains("default_product.png"))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, productInDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ImageFile.CopyToAsync(fileStream);
                    }
                    viewModel.Product.ImageUrl = "/images/products/" + uniqueFileName;
                }
                else
                {
                    viewModel.Product.ImageUrl = productInDb.ImageUrl;
                }

                try
                {
                    _context.Products.Update(viewModel.Product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == viewModel.Product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Message"] = "Produto atualizado com sucesso!";
                return RedirectToAction("List");
            }

            viewModel.Categories = await GetCategorySelectListAsync();
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
    }
}