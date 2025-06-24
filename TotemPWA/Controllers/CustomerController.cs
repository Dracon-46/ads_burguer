using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var customizations = await _context.Customizes
                .Include(c => c.Ingredient)
                .Include(c => c.OrderItem)
                .ToListAsync();

            return View(customizations);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Ingredients"] = new SelectList(_context.Ingredients, "Id", "Name");
            ViewData["OrderItems"] = new SelectList(_context.OrderItems, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customize customize)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Ingredients"] = new SelectList(_context.Ingredients, "Id", "Name");
                ViewData["OrderItems"] = new SelectList(_context.OrderItems, "Id", "Id");
                return View(customize);
            }

            _context.Customizes.Add(customize);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
    }
}
