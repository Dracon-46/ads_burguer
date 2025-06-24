using Microsoft.AspNetCore.Mvc;
using TotemPWA.Models;
using TotemPWA.Data;

namespace TotemPWA.Controllers
{
    [Route("[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Simula um carrinho temporário
            var cart = HttpContext.Session.GetString("Cart") ?? "[]";
            return View("Cart", cart);
        }

        [HttpPost]
        public IActionResult AddItem(int productId)
        {
            // Lógica para adicionar produto no carrinho (salvar em Session ou banco)
            TempData["Message"] = $"Produto {productId} adicionado ao carrinho!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }
    }
}
