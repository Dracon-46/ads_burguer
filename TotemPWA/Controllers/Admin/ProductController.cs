using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Linq;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.ViewModels;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")] // Garante o roteamento correto
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context; // ALTERADO: AppDbContext para ApplicationDbContext
        public ProductController(ApplicationDbContext context) // ALTERADO: AppDbContext para ApplicationDbContext
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            // Inclui a categoria para exibição
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(viewModel.Product);
                await _context.SaveChangesAsync();
                return RedirectToAction("List");
            }

            // Se o modelo for inválido, recarrega as categorias para o dropdown
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

            if (ModelState.IsValid)
            {
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
                return RedirectToAction("List");
            }

            // Recarregar categorias se inválido
            viewModel.Categories = await GetCategorySelectListAsync();
            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.Promotions) // Verifica promoções
                                .Include(p => p.Additionals) // Verifica adicionais
                                .Include(p => p.ProductCombos) // Verifica combos onde é o principal
                                .Include(p => p.ComposedCombos) // Verifica combos onde é um componente
                                .Include(p => p.OrderItems) // Verifica itens de pedido
                                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            // Lógica para evitar exclusão se o produto estiver em uso
            if (product.Promotions != null && product.Promotions.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este produto porque ele está associado a promoções.";
                return RedirectToAction(nameof(List));
            }
            if (product.Additionals != null && product.Additionals.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este produto porque ele está associado a adicionais.";
                return RedirectToAction(nameof(List));
            }
            if ((product.ProductCombos != null && product.ProductCombos.Any()) || (product.ComposedCombos != null && product.ComposedCombos.Any()))
            {
                TempData["ErrorMessage"] = "Não é possível excluir este produto porque ele está associado a combos.";
                return RedirectToAction(nameof(List));
            }
            if (product.OrderItems != null && product.OrderItems.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este produto porque ele está associado a itens de pedido.";
                return RedirectToAction(nameof(List));
            }


            return View(product);
        }

        [HttpPost("{id}")]
        [ActionName("ConfirmDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
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