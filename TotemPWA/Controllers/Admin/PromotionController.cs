using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.ViewModels;
using System; // Adicione este using para DateTime.Today
using System.Linq; // Adicione este using para métodos Linq como Any() e ToList()

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")] // Garante o roteamento correto
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PromotionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            // Inclui o produto para exibição
            var promotions = await _context.Promotions.Include(p => p.Product).ToListAsync();
            return View(promotions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new PromotionViewModel
            {
                Products = await GetProductSelectListAsync(),
                ValidUntil = DateTime.Today // <<< ESTA É A LINHA QUE FAZ A DATA APARECER CORRETA!
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionViewModel model)
        {
            // Verifica se já existe uma promoção para o mesmo produto com data de validade futura
            if (await _context.Promotions.AnyAsync(p => p.ProductId == model.ProductId && p.ValidUntil >= DateTime.Today))
            {
                ModelState.AddModelError("ProductId", "Já existe uma promoção ativa para este produto.");
            }

            if (!ModelState.IsValid)
            {
                model.Products = await GetProductSelectListAsync(); // Recarrega para manter a lista
                return View(model);
            }

            var promotion = new Promotion
            {
                ProductId = model.ProductId,
                Percent = model.Percent,
                ValidUntil = model.ValidUntil
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Promoção criada com sucesso!"; // Mensagem de sucesso
            return RedirectToAction("List");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            var viewModel = new PromotionViewModel
            {
                Id = promo.Id,
                ProductId = promo.ProductId,
                Percent = promo.Percent,
                ValidUntil = promo.ValidUntil,
                Products = await GetProductSelectListAsync()
            };
            return View(viewModel);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PromotionViewModel model)
        {
            // Verifica se existe outra promoção ativa para o mesmo produto (excluindo a própria)
            if (await _context.Promotions.AnyAsync(p => p.ProductId == model.ProductId && p.ValidUntil >= DateTime.Today && p.Id != model.Id))
            {
                ModelState.AddModelError("ProductId", "Já existe outra promoção ativa para este produto.");
            }

            if (!ModelState.IsValid)
            {
                model.Products = await GetProductSelectListAsync(); // Recarrega para manter a lista
                return View(model);
            }

            var promo = await _context.Promotions.FindAsync(model.Id);
            if (promo == null) return NotFound();

            // Atualiza as propriedades
            promo.ProductId = model.ProductId;
            promo.Percent = model.Percent;
            promo.ValidUntil = model.ValidUntil;

            try
            {
                _context.Update(promo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PromotionExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["Message"] = "Promoção atualizada com sucesso!"; // Mensagem de sucesso
            return RedirectToAction("List");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var promo = await _context.Promotions.Include(p => p.Product).FirstOrDefaultAsync(p => p.Id == id);
            if (promo == null) return NotFound();

            return View(promo);
        }

        // CORREÇÃO AQUI: Renomeado ActionName de "DeleteConfirmed" para "Delete" para corresponder ao HttpPost,
        // ou use apenas [HttpPost] e renomeie o método para Delete
        [HttpPost("{id}"), ActionName("Delete")] // <<-- AÇÃO POST PARA DELETE AGORA SE CHAMA "Delete" NO ATRIBUTO
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // O nome do método pode permanecer o mesmo ou ser 'Delete'
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Promoção excluída com sucesso!"; // Mensagem de sucesso
            return RedirectToAction("List");
        }

        // Método auxiliar para carregar a lista de produtos para SelectListItems
        private async Task<List<SelectListItem>> GetProductSelectListAsync()
        {
            return await _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync();
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.Id == id);
        }
    }
}