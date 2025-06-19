using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")] // Garante o roteamento correto
    public class CupomController : Controller
    {
        private readonly ApplicationDbContext _context; // ALTERADO: AppDbContext para ApplicationDbContext

        public CupomController(ApplicationDbContext context) // ALTERADO: AppDbContext para ApplicationDbContext
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var cupons = await _context.Cupons.ToListAsync();
            return View(cupons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cupom cupom)
        {
            // Verifica se um cupom com o mesmo código já existe
            if (_context.Cupons.Any(c => c.Code == cupom.Code))
            {
                ModelState.AddModelError("Code", "Um cupom com este código já existe.");
            }

            if (!ModelState.IsValid) return View(cupom);

            _context.Cupons.Add(cupom);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null) return NotFound();

            return View(cupom);
        }

        [HttpPost("{id}")] // Adicionado rota com ID
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cupom cupom)
        {
            if (id != cupom.Id) return BadRequest();

            // Verifica se um cupom com o mesmo código já existe, excluindo o próprio cupom
            if (_context.Cupons.Any(c => c.Code == cupom.Code && c.Id != cupom.Id))
            {
                ModelState.AddModelError("Code", "Um cupom com este código já existe.");
            }

            if (!ModelState.IsValid) return View(cupom);

            try
            {
                _context.Update(cupom);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cupons.Any(c => c.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cupom = await _context.Cupons.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id);
            if (cupom == null) return NotFound();

            // Lógica para evitar exclusão se o cupom estiver em uso
            if (cupom.Orders != null && cupom.Orders.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este cupom porque ele está associado a pedidos existentes.";
                return RedirectToAction(nameof(List));
            }

            return View(cupom);
        }

        [HttpPost("{id}"), ActionName("DeleteConfirmed")] // Adicionado rota com ID e ActionName
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null) return NotFound();

            _context.Cupons.Remove(cupom);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
    }
}