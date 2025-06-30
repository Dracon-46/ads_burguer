// Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Utilities;
using TotemPWA.Models.ViewModels;
using System.Linq;
using System; // Adicione este using para Guid

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

        // MODIFICADO: AddItem agora recebe um GUID para aumentar a quantidade de um item ESPECÍFICO
        // IMPORTANTE: Para ADICIONAR UM NOVO ITEM PERSONALIZADO, use a ação SalvarPersonalizacao do HomeController.
        // Esta ação é para aumentar a quantidade de um item JÁ EXISTENTE E PERSONALIZADO NO CARRINHO.
        [HttpPost]
        public IActionResult AddItem(Guid cartItemId) // Recebe o GUID agora
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.CartItemId == cartItemId); // Busca pelo CartItemId

            if (item != null)
            {
                item.Quantity++;
                HttpContext.Session.SetObject("Cart", cart);
            }
            // Se o item não for encontrado, ele não deve ser adicionado aqui, mas sim pela Personalização
            return RedirectToAction("Index");
        }

        // MODIFICADO: DecreaseItem agora recebe um GUID
        [HttpPost]
        public IActionResult DecreaseItem(Guid cartItemId) // Recebe o GUID agora
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.CartItemId == cartItemId); // Busca pelo CartItemId

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

        // MODIFICADO: RemoveItem agora recebe um GUID para remover a instância COMPLETA de um item personalizado
        [HttpPost]
        public IActionResult RemoveItem(Guid cartItemId) // Recebe o GUID agora
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.CartItemId == cartItemId); // Busca pelo CartItemId
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
            return Json(new
            {
                totalItems = cart.Sum(item => item.Quantity),
                totalPrice = cart.Sum(item => item.Price * item.Quantity)
            });
        }
    }
}