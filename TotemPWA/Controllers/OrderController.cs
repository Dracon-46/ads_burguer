using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var orders = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Cupom)
                .Include(o => o.Items)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Cupom)
                .Include(o => o.Items)           // Items precisa ser não-nullable
                    .ThenInclude(i => i.Product) // Produto do item
                .FirstOrDefaultAsync(o => o.Id == id);


            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
    }
}
