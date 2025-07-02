// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels;
using TotemPWA.ViewModels;
using TotemPWA.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

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
            var product = await _context.Products
                .Include(p => p.Additionals!)
                .ThenInclude(pa => pa.Ingredient)
                .FirstOrDefaultAsync(p => p.Id == model.ProdutoId);

            if (product == null) return NotFound("Produto não encontrado.");

            decimal personalizedPrice = product.Price; // Começa com o preço base do produto
            var personalizationSummary = new List<string>();

            var manipulatedIngredientsWithQuantity = new Dictionary<int, int>();

            // Itera sobre os ingredientes manipulados enviados do formulário
            foreach (var entry in model.IngredientesManipuladosQuantidades)
            {
                var ingredientId = entry.Key;
                var finalQuantity = entry.Value; // Quantidade final para este ingrediente

                // Encontra o Additional correspondente para verificar as flags e obter o Ingredient
                var additional = product.Additionals?.FirstOrDefault(a => a.IngredientId == ingredientId);

                if (additional != null && additional.Ingredient != null)
                {
                    // Valida o limite do ingrediente (se a quantidade for de adição)
                    if (additional.CanBeAdded && finalQuantity > additional.Ingredient.Limit)
                    {
                        finalQuantity = additional.Ingredient.Limit; // Limita à quantidade máxima permitida
                    }

                    // Calcula o impacto no preço e adiciona ao resumo
                    if (additional.CanBeAdded)
                    {
                        personalizedPrice += additional.Ingredient.Price * finalQuantity;
                        if (finalQuantity > 0)
                        {
                            personalizationSummary.Add($"{finalQuantity}x com {additional.Ingredient.Name}");
                        }
                    }
                    else if (additional.IsDefault && additional.CanBeRemoved && finalQuantity <= 0)
                    {
                        personalizationSummary.Add($"sem {additional.Ingredient.Name}");
                    }

                    manipulatedIngredientsWithQuantity[ingredientId] = finalQuantity;
                }
            }

            var summaryText = personalizationSummary.Any()
                ? "(" + string.Join(", ", personalizationSummary) + ")"
                : "";

            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            CartItemViewModel? cartItemToModify = null;
            if (model.CartItemId != Guid.Empty)
            {
                cartItemToModify = cart.FirstOrDefault(ci => ci.CartItemId == model.CartItemId);
            }

            if (cartItemToModify == null) // Novo item personalizado
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = personalizedPrice,
                    Image = product.Image ?? product.ImageUrl ?? "/images/products/default_product.png",
                    Quantity = 1,
                    CartItemId = Guid.NewGuid(),
                    ManipulatedIngredientsWithQuantity = manipulatedIngredientsWithQuantity,
                    PersonalizationSummary = summaryText
                });
            }
            else // Editando item existente no carrinho
            {
                cartItemToModify.Price = personalizedPrice;
                cartItemToModify.ManipulatedIngredientsWithQuantity = manipulatedIngredientsWithQuantity;
                cartItemToModify.PersonalizationSummary = summaryText;
            }

            HttpContext.Session.SetObject("Cart", cart);
            TempData["Message"] = "Produto personalizado e adicionado/atualizado no carrinho!";
            return RedirectToAction("Index", "Cart");
        }


        [HttpGet]
        public async Task<IActionResult> PersonalizarProdutos(int productId, Guid? cartItemId)
        {
            var product = await _context.Products
                                .Include(p => p.Additionals!) // Inclua todos os Additionals [cite: 73, 74, 101]
                                .ThenInclude(pa => pa.Ingredient) // E os Ingredientes relacionados [cite: 74, 101]
                                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
               return NotFound("Produto não encontrado."); 

            var viewModel = new PersonalizarProdutoViewModel
            {
                Produto = product,
                CartItemId = cartItemId ?? Guid.Empty, 
                ProdutoAdditionals = product.Additionals // Passa todos os additionals do produto para a view
            };

            // Se for edição, pré-preenche o ViewModel com as quantidades manipuladas do carrinho
            if (cartItemId.HasValue && cartItemId.Value != Guid.Empty)
            {
                var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
                var existingCartItem = cart.FirstOrDefault(ci => ci.CartItemId == cartItemId.Value);
                if (existingCartItem != null)
                {
                    // Copia as quantidades manipuladas existentes para pré-preencher a interface
                    viewModel.QuantidadesManipuladas = existingCartItem.ManipulatedIngredientsWithQuantity ?? new Dictionary<int, int>();
                }
            }

           return View("~/Views/Home/PersonalizarProdutos.cshtml", viewModel);
        }
    }
}