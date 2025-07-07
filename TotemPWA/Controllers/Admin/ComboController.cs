using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels; // Certifique-se de que o namespace do ViewModel está correto

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class ComboController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComboController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Combo/List
        public async Task<IActionResult> List()
        {
            // Agrupamos por ProductComboId para ter uma entrada por "combo principal"
            var comboProducts = await _context.Combos
                .Include(c => c.ProductCombo) // O produto que é o combo
                .Include(c => c.Product)      // Os produtos que compõem o combo
                .GroupBy(c => c.ProductComboId)
                .Select(g => new ComboViewModel
                {
                    ProductComboId = g.Key,
                    ComboProductName = g.First().ProductCombo != null ? g.First().ProductCombo.Name : "N/A",
                    IncludedProducts = g.Select(c => new IncludedProductViewModel
                    {
                        ProductId = c.ProductId,
                        ProductName = c.Product != null ? c.Product.Name : "N/A",
                        ProductPrice = c.Product != null ? c.Product.Price : 0M
                    }).ToList()
                })
                .ToListAsync();

            // Calcula o preço total do combo (soma dos preços dos produtos incluídos)
            foreach (var comboVm in comboProducts)
            {
                comboVm.ComboPrice = comboVm.IncludedProducts.Sum(ip => ip.ProductPrice);
            }

            return View(comboProducts);
        }

        // GET: Admin/Combo/Create
        public async Task<IActionResult> Create()
        {
            var products = await _context.Products
                                         .OrderBy(p => p.Name)
                                         .Select(p => new SelectListItem
                                         {
                                             Value = p.Id.ToString(),
                                             Text = $"{p.Name} (R$ {p.Price:F2})" // Exibe nome e preço
                                         })
                                         .ToListAsync();

            var viewModel = new ComboViewModel
            {
                AvailableProducts = products
            };
            return View(viewModel);
        }

        // POST: Admin/Combo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComboViewModel viewModel)
        {
            // Remove a validação para o nome e preço do combo, pois eles são apenas para exibição na lista.
            ModelState.Remove(nameof(viewModel.ComboProductName));
            ModelState.Remove(nameof(viewModel.ComboPrice));
            ModelState.Remove(nameof(viewModel.IncludedProducts));

            if (!ModelState.IsValid)
            {
                // Se o modelo for inválido, repopule os produtos disponíveis e retorne a view
                viewModel.AvailableProducts = await GetProductSelectListAsync();
                return View(viewModel);
            }

            if (viewModel.SelectedProductIds == null || !viewModel.SelectedProductIds.Any())
            {
                ModelState.AddModelError("SelectedProductIds", "Você deve selecionar pelo menos um produto para o combo.");
                viewModel.AvailableProducts = await GetProductSelectListAsync();
                return View(viewModel);
            }

            foreach (var productId in viewModel.SelectedProductIds)
            {
                if (viewModel.ProductComboId == productId)
                {
                    ModelState.AddModelError("SelectedProductIds", "Um produto não pode ser um combo de si mesmo.");
                    viewModel.AvailableProducts = await GetProductSelectListAsync();
                    return View(viewModel);
                }

                var comboEntry = new Combo
                {
                    ProductComboId = viewModel.ProductComboId,
                    ProductId = productId
                };
                _context.Combos.Add(comboEntry);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Combo criado com sucesso!";
            return RedirectToAction(nameof(List));
        }

        // GET: Admin/Combo/Edit/{id} (id aqui é o ProductComboId)
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var existingCombos = await _context.Combos
                                               .Where(c => c.ProductComboId == id)
                                               .ToListAsync();

            if (!existingCombos.Any())
            {
                return NotFound();
            }

            var comboProduct = await _context.Products.FindAsync(id);
            if (comboProduct == null)
            {
                return NotFound(); // O produto principal do combo não existe
            }

            var viewModel = new ComboViewModel
            {
                ProductComboId = id,
                ComboProductName = comboProduct.Name,
                SelectedProductIds = existingCombos.Select(c => c.ProductId).ToList(),
                AvailableProducts = await GetProductSelectListAsync()
            };

            return View(viewModel);
        }

        // POST: Admin/Combo/Edit/{id}
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ComboViewModel viewModel)
        {
            if (id != viewModel.ProductComboId)
            {
                return BadRequest("ID do combo inválido.");
            }

            // Remove a validação para o nome e preço do combo, pois eles são apenas para exibição na lista.
            ModelState.Remove(nameof(viewModel.ComboProductName));
            ModelState.Remove(nameof(viewModel.ComboPrice));
            ModelState.Remove(nameof(viewModel.IncludedProducts));

            if (!ModelState.IsValid)
            {
                viewModel.AvailableProducts = await GetProductSelectListAsync();
                return View(viewModel);
            }

            if (viewModel.SelectedProductIds == null || !viewModel.SelectedProductIds.Any())
            {
                ModelState.AddModelError("SelectedProductIds", "Você deve selecionar pelo menos um produto para o combo.");
                viewModel.AvailableProducts = await GetProductSelectListAsync();
                return View(viewModel);
            }

            // Remover combos existentes para este ProductComboId
            var existingCombos = await _context.Combos
                                                .Where(c => c.ProductComboId == id)
                                                .ToListAsync();
            _context.Combos.RemoveRange(existingCombos);
            await _context.SaveChangesAsync(); // Salvar para garantir a remoção antes de adicionar novos

            // Adicionar novos combos com base nas seleções
            foreach (var productId in viewModel.SelectedProductIds)
            {
                if (viewModel.ProductComboId == productId)
                {
                    ModelState.AddModelError("SelectedProductIds", "Um produto não pode ser um combo de si mesmo.");
                    viewModel.AvailableProducts = await GetProductSelectListAsync();
                    return View(viewModel);
                }

                var comboEntry = new Combo
                {
                    ProductComboId = viewModel.ProductComboId,
                    ProductId = productId
                };
                _context.Combos.Add(comboEntry);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Combo atualizado com sucesso!";
            return RedirectToAction(nameof(List));
        }

        // GET: Admin/Combo/Delete/{id} (id aqui é o ProductComboId)
        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comboProduct = await _context.Products.FindAsync(id);
            if (comboProduct == null)
            {
                return NotFound();
            }

            var existingCombos = await _context.Combos
                                               .Include(c => c.Product)
                                               .Where(c => c.ProductComboId == id)
                                               .ToListAsync();

            if (!existingCombos.Any())
            {
                // Se não há combos, ainda podemos mostrar a tela de confirmação para o produto principal
                var emptyComboVm = new ComboViewModel
                {
                    ProductComboId = id,
                    ComboProductName = comboProduct.Name
                };
                return View(emptyComboVm);
            }

            var viewModel = new ComboViewModel
            {
                ProductComboId = id,
                ComboProductName = comboProduct.Name,
                IncludedProducts = existingCombos.Select(c => new IncludedProductViewModel
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product != null ? c.Product.Name : "N/A",
                    ProductPrice = c.Product != null ? c.Product.Price : 0M
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Admin/Combo/Delete/{id}
        [HttpPost("{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combosToRemove = await _context.Combos
                                                .Where(c => c.ProductComboId == id)
                                                .ToListAsync();

            if (combosToRemove.Any())
            {
                _context.Combos.RemoveRange(combosToRemove);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Combo excluído com sucesso!";
            }
            else
            {
                TempData["Message"] = "Nenhum combo encontrado para exclusão.";
            }
            return RedirectToAction(nameof(List));
        }

        private async Task<List<SelectListItem>> GetProductSelectListAsync()
        {
            return await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} (R$ {p.Price:F2})"
                }).ToListAsync();
        }
    }
}