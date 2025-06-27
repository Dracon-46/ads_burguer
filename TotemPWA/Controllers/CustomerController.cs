using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace TotemPWA.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarPersonalizacao(PersonalizarProdutoInputModel model)
        {
            var orderItem = await _context.OrderItems.FindAsync(model.OrderItemId);
            if (orderItem == null) return NotFound();

            if (model.TipoProduto == "lanche")
            {
                foreach (var id in model.IngredientesParaAdicionar)
                {
                    _context.Customizes.Add(new Customize
                    {
                        OrderItemId = model.OrderItemId,
                        IngredientId = id,
                        Type = "adicionar"
                    });
                }

                foreach (var id in model.IngredientesParaRemover)
                {
                    _context.Customizes.Add(new Customize
                    {
                        OrderItemId = model.OrderItemId,
                        IngredientId = id,
                        Type = "remover"
                    });
                }
            }
            else if (model.TipoProduto == "bebida" || model.TipoProduto == "acompanhamento")
            {
                _context.Customizes.Add(new Customize
                {
                    OrderItemId = model.OrderItemId,
                    IngredientId = 0,
                    Type = model.TamanhoSelecionado ?? "padrão"
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Cart");
        }

        [HttpGet]
        public async Task<IActionResult> PersonalizarCombo(int orderItemId)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

            if (orderItem == null || orderItem.Product == null)
                return NotFound("Item do pedido não encontrado.");

            // Identifica o tipo do produto por nome ou CategoryId (ajuste conforme seu modelo)
            string tipo = "";
            if (orderItem.Product.Name.ToLower().Contains("bebida"))
                tipo = "bebida";
            else if (orderItem.Product.Name.ToLower().Contains("acomp"))
                tipo = "acompanhamento";
            else
                tipo = "lanche"; // padrão

            // Ingredientes (só para lanche)
            var ingredientes = tipo == "lanche"
                ? await _context.Ingredients.ToListAsync()
                : new List<Ingredient>();

            // Tamanhos (só para bebida ou acompanhamento)
            var tamanhos = tipo switch
            {
                "bebida" => new List<string> { "300ml", "500ml", "700ml" },
                "acompanhamento" => new List<string> { "Pequeno", "Médio", "Grande" },
                _ => new List<string>()
            };

            var viewModel = new PersonalizarProdutoViewModel
            {
                Produto = orderItem.Product,
                TipoProduto = tipo,
                Ingredientes = ingredientes,
                Tamanhos = tamanhos,
                OrderItemId = orderItem.Id
            };

            return View("PersonalizarCombo", viewModel);
        }

    }
}
