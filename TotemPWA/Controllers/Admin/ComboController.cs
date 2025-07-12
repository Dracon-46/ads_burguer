using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class ComboController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ComboController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var combos = await _context.Products
                .Where(p => p.ProductCombos != null && p.ProductCombos.Any())
                .Include(p => p.ProductCombos!)
                    .ThenInclude(pc => pc.Product)
                .Select(p => new ComboViewModel
                {
                    ProductComboId = p.Id,
                    ComboProductName = p.Name,
                    ComboPrice = p.Price,
                    ComboDescription = p.Description,
                    ImageUrl = p.ImageUrl,
                    IncludedProducts = p.ProductCombos!.Select(pc => new IncludedProductViewModel
                    {
                        ProductId = pc.Product!.Id,
                        ProductName = pc.Product.Name,
                        ProductPrice = pc.Product.Price
                    }).ToList()
                })
                .ToListAsync();

            return View(combos);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var availableProducts = await _context.Products
                .Where(p => p.Active)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} - R$ {p.Price:F2}"
                })
                .ToListAsync();

            var viewModel = new ComboViewModel
            {
                AvailableProducts = availableProducts,
                IsEdit = false
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComboViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Criar o produto principal do combo
                    var comboProduct = new Product
                    {
                        Name = viewModel.ComboProductName,
                        Description = viewModel.ComboDescription,
                        Price = viewModel.ComboPrice, // Usar o preço definido pelo usuário
                        Active = true,
                        CategoryId = 1 // Você pode criar uma categoria específica para combos
                    };

                    // Upload da imagem se fornecida
                    if (viewModel.ImageFile != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ImageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ImageFile.CopyToAsync(fileStream);
                        }
                        
                        comboProduct.ImageUrl = "/images/products/" + uniqueFileName;
                    }

                    _context.Products.Add(comboProduct);
                    await _context.SaveChangesAsync();

                    // Criar os relacionamentos do combo
                    foreach (var productId in viewModel.SelectedProductIds)
                    {
                        var combo = new Combo
                        {
                            ProductComboId = comboProduct.Id,
                            ProductId = productId
                        };
                        _context.Combos.Add(combo);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Message"] = "Combo criado com sucesso!";
                    return RedirectToAction(nameof(List));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Erro ao criar o combo. Tente novamente.");
                }
            }

            // Recarregar produtos disponíveis em caso de erro
            viewModel.AvailableProducts = await _context.Products
                .Where(p => p.Active)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} - R$ {p.Price:F2}"
                })
                .ToListAsync();

            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var comboProduct = await _context.Products
                .Include(p => p.ProductCombos!)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (comboProduct == null)
                return NotFound();

            var availableProducts = await _context.Products
                .Where(p => p.Active && p.Id != id) // Excluir o próprio combo dos produtos disponíveis
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} - R$ {p.Price:F2}"
                })
                .ToListAsync();

            var viewModel = new ComboViewModel
            {
                ProductComboId = comboProduct.Id,
                ComboProductName = comboProduct.Name,
                ComboPrice = comboProduct.Price,
                ComboDescription = comboProduct.Description,
                ImageUrl = comboProduct.ImageUrl,
                SelectedProductIds = comboProduct.ProductCombos?.Select(pc => pc.ProductId).ToList() ?? new List<int>(),
                AvailableProducts = availableProducts,
                IsEdit = true,
                IncludedProducts = comboProduct.ProductCombos?.Select(pc => new IncludedProductViewModel
                {
                    ProductId = pc.Product!.Id,
                    ProductName = pc.Product.Name,
                    ProductPrice = pc.Product.Price
                }).ToList() ?? new List<IncludedProductViewModel>()
            };

            return View(viewModel);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ComboViewModel viewModel)
        {
            if (id != viewModel.ProductComboId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var comboProduct = await _context.Products
                        .Include(p => p.ProductCombos)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (comboProduct == null)
                        return NotFound();

                    // Atualizar as propriedades do produto combo
                    comboProduct.Name = viewModel.ComboProductName;
                    comboProduct.Description = viewModel.ComboDescription;
                    comboProduct.Price = viewModel.ComboPrice; // Usar o preço definido pelo usuário

                    // Upload da nova imagem se fornecida
                    if (viewModel.ImageFile != null)
                    {
                        // Remover imagem anterior se existir
                        if (!string.IsNullOrEmpty(comboProduct.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, comboProduct.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ImageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ImageFile.CopyToAsync(fileStream);
                        }
                        
                        comboProduct.ImageUrl = "/images/products/" + uniqueFileName;
                    }

                    // Remover todos os relacionamentos antigos do combo
                    var existingCombos = await _context.Combos
                        .Where(c => c.ProductComboId == id)
                        .ToListAsync();
                    
                    _context.Combos.RemoveRange(existingCombos);

                    // Adicionar os novos relacionamentos
                    foreach (var productId in viewModel.SelectedProductIds)
                    {
                        var combo = new Combo
                        {
                            ProductComboId = id,
                            ProductId = productId
                        };
                        _context.Combos.Add(combo);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Message"] = "Combo atualizado com sucesso!";
                    return RedirectToAction(nameof(List));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Erro ao atualizar o combo. Tente novamente.");
                }
            }

            // Recarregar produtos disponíveis em caso de erro
            viewModel.AvailableProducts = await _context.Products
                .Where(p => p.Active && p.Id != id)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} - R$ {p.Price:F2}"
                })
                .ToListAsync();

            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comboProduct = await _context.Products
                .Include(p => p.ProductCombos!)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (comboProduct == null)
                return NotFound();

            var viewModel = new ComboViewModel
            {
                ProductComboId = comboProduct.Id,
                ComboProductName = comboProduct.Name,
                ComboPrice = comboProduct.Price,
                ComboDescription = comboProduct.Description,
                ImageUrl = comboProduct.ImageUrl,
                IncludedProducts = comboProduct.ProductCombos?.Select(pc => new IncludedProductViewModel
                {
                    ProductId = pc.Product!.Id,
                    ProductName = pc.Product.Name,
                    ProductPrice = pc.Product.Price
                }).ToList() ?? new List<IncludedProductViewModel>()
            };

            return View(viewModel);
        }

        [HttpPost("{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var comboProduct = await _context.Products
                    .Include(p => p.ProductCombos)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (comboProduct == null)
                    return NotFound();

                // Remover todos os relacionamentos do combo
                if (comboProduct.ProductCombos != null)
                {
                    _context.Combos.RemoveRange(comboProduct.ProductCombos);
                }

                // Remover imagem se existir
                if (!string.IsNullOrEmpty(comboProduct.ImageUrl))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, comboProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Remover o produto combo
                _context.Products.Remove(comboProduct);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Message"] = "Combo excluído com sucesso!";
                return RedirectToAction(nameof(List));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Erro ao excluir o combo. Tente novamente.";
                return RedirectToAction(nameof(List));
            }
        }
    }
}