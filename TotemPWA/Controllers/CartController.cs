using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Utilities;
using TotemPWA.Models.ViewModels;

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
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            return View("Cart", cart);
        }

        [HttpPost]
        public IActionResult AddItem(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
                item.Quantity++;
            else
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Image = product.Image,
                    Quantity = 1
                });

            HttpContext.Session.SetObject("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetCartSummary()
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            return Json(new {
                totalItems = cart.Sum(item => item.Quantity),
                totalPrice = cart.Sum(item => item.Price * item.Quantity)
            });
        }
    }
}
