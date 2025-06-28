using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Utilities;
using TotemPWA.Models.ViewModels;
using System.Linq; 
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

        // Ação original RemoveItem (mantida, mas agora serve para remover item COMPLETO)
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

        // --- NOVA AÇÃO: DecreaseItem ---
        // Esta ação é chamada quando o usuário clica no botão de subtração para reduzir a quantidade em 1.
        [HttpPost]
        public IActionResult DecreaseItem(int productId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--; // Decrementa a quantidade em 1
                }
                else
                {
                    // Se a quantidade for 1, remove o item completamente do carrinho
                    cart.Remove(item);
                }
                HttpContext.Session.SetObject("Cart", cart); // Salva o carrinho atualizado na sessão
            }
            
            return RedirectToAction("Index"); // Redireciona de volta para a tela do carrinho
        }
        // --- FIM DA NOVA AÇÃO ---


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