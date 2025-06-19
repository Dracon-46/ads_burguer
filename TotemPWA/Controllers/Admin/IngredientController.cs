using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")] // Garante o roteamento correto
    public class IngredientController : Controller
    {
        private readonly ApplicationDbContext _context; // ALTERADO: AppDbContext para ApplicationDbContext

        public IngredientController(ApplicationDbContext context) // ALTERADO: AppDbContext para ApplicationDbContext
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredient ingredient)
        {
            // Verifica se um ingrediente com o mesmo nome já existe
            if (_context.Ingredients.Any(i => i.Name == ingredient.Name))
            {
                ModelState.AddModelError("Name", "Um ingrediente com este nome já existe.");
            }

            if (!ModelState.IsValid) return View(ingredient);

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();

            return View(ingredient);
        }

        [HttpPost("{id}")] // Adicionado rota com ID
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ingredient ingredient)
        {
            if (id != ingredient.Id) return BadRequest();

            // Verifica se um ingrediente com o mesmo nome já existe, excluindo o próprio ingrediente
            if (_context.Ingredients.Any(i => i.Name == ingredient.Name && i.Id != ingredient.Id))
            {
                ModelState.AddModelError("Name", "Um ingrediente com este nome já existe.");
            }

            if (!ModelState.IsValid) return View(ingredient);

            try
            {
                _context.Update(ingredient);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ingredients.Any(i => i.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = await _context.Ingredients
                                .Include(i => i.Additionals) // Para verificar se está em Additional
                                .Include(i => i.Customizations) // Para verificar se está em Customize
                                .FirstOrDefaultAsync(i => i.Id == id);
            if (ingredient == null) return NotFound();

            // Lógica para evitar exclusão se o ingrediente estiver em uso
            if (ingredient.Additionals != null && ingredient.Additionals.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este ingrediente porque ele está associado a adicionais de produtos.";
                return RedirectToAction(nameof(List));
            }
            if (ingredient.Customizations != null && ingredient.Customizations.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este ingrediente porque ele está associado a customizações de pedidos.";
                return RedirectToAction(nameof(List));
            }

            return View(ingredient);
        }

        [HttpPost("{id}"), ActionName("DeleteConfirmed")] // Adicionado rota com ID e ActionName
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
    }
}