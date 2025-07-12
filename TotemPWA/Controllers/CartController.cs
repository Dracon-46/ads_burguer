using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Utilities; // Certifique-se de que esta classe de extensão para Session está disponível
using TotemPWA.Models.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System;
using TotemPWA.Models; // Adicionar para Product e Additional

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
        public async Task<IActionResult> Index() // Torna a ação assíncrona
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var itemsToRemove = new List<CartItemViewModel>();

            // Pré-carrega as promoções ativas para otimização, se necessário.
            var activePromotions = await _context.Promotions
                .Where(p => p.ValidUntil >= DateTime.Today)
                .ToListAsync();

            foreach (var item in cart)
            {
                var product = await _context.Products
                                    .Include(p => p.Additionals!)
                                        .ThenInclude(pa => pa.Ingredient)
                                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                {
                    itemsToRemove.Add(item);
                    continue;
                }

                // *** NOVA LÓGICA: Verificar se é um combo ***
                var isCombo = await _context.Combos.AnyAsync(c => c.ProductComboId == item.ProductId);

                if (isCombo)
                {
                    // *** PARA COMBOS: Não recalcular o preço, apenas atualizar o resumo se necessário ***
                    // O preço do combo já foi calculado corretamente no SalvarPersonalizacaoCombo
                    // Apenas garantir que o resumo está correto (opcional)
                    
                    // Se você quiser revalidar o resumo do combo, pode fazer aqui
                    // Por enquanto, vamos manter o preço e resumo que já estão salvos
                    continue; // Pula para o próximo item
                }

                // *** LÓGICA ORIGINAL PARA PRODUTOS INDIVIDUAIS ***
                // 1. Começa o preço recalculado com o preço base do produto.
                decimal recalculatedPrice = product.Price;

                // 2. Aplica a promoção ao preço base, se houver uma promoção ativa.
                var promotion = activePromotions.FirstOrDefault(p => p.ProductId == product.Id);
                if (promotion != null)
                {
                    // Calcula o preço com desconto (Ex: Price * (1 - Percent/100))
                    recalculatedPrice = recalculatedPrice * (1 - promotion.Percent / 100);
                }

                var tempPersonalizationSummary = new List<string>();

                // Obtém as adições padrão do produto configuradas pelo administrador
                var productStandardAdditions = product.Additionals?.ToDictionary(a => a.IngredientId) ?? new Dictionary<int, Additional>();

                // Itera sobre os ingredientes manipulados que estão no item do carrinho
                if (item.ManipulatedIngredientsWithQuantity != null) // Garante que o dicionário não seja nulo
                {
                    foreach (var entry in item.ManipulatedIngredientsWithQuantity)
                    {
                        var ingredientId = entry.Key;
                        var finalQuantity = entry.Value; // Quantidade final que o cliente escolheu

                        if (productStandardAdditions.TryGetValue(ingredientId, out var additional) && additional.Ingredient != null)
                        {
                            var standardQuantity = additional.Quantity; // Quantidade padrão do ingrediente no produto
                            var ingredientPrice = additional.Ingredient.Price; // Preço do ingrediente

                            // Lógica para recalcular o preço baseado na diferença de quantidade
                            if (finalQuantity > standardQuantity)
                            {
                                recalculatedPrice += ingredientPrice * (finalQuantity - standardQuantity);
                                tempPersonalizationSummary.Add($"{finalQuantity}x {additional.Ingredient.Name}");
                            }
                            else if (finalQuantity < standardQuantity)
                            {
                                // Reflete a remoção no resumo
                                tempPersonalizationSummary.Add($"removido {standardQuantity - finalQuantity}x de {additional.Ingredient.Name}");
                                // O preço NÃO é reduzido aqui, mantendo a consistência com a lógica de SalvarPersonalizacao.
                            }
                            else if (finalQuantity == standardQuantity && finalQuantity > 0)
                            {
                                tempPersonalizationSummary.Add($"{finalQuantity}x {additional.Ingredient.Name}");
                            }
                            else if (finalQuantity == 0 && standardQuantity > 0)
                            {
                                tempPersonalizationSummary.Add($"sem {additional.Ingredient.Name}"); // Removeu completamente um ingrediente padrão
                            }
                        }
                        else
                        {
                            // Ingrediente no carrinho que não é mais um Adicional do produto ou é um "extra" não padrão.
                            if (finalQuantity > 0)
                            {
                                var unknownIngredient = await _context.Ingredients.FindAsync(ingredientId);
                                if (unknownIngredient != null)
                                {
                                    recalculatedPrice += unknownIngredient.Price * finalQuantity;
                                    tempPersonalizationSummary.Add($"extra {finalQuantity}x {unknownIngredient.Name}");
                                }
                                else
                                {
                                    // Registra ou lida com ingrediente desconhecido (ex: se foi excluído do DB)
                                    tempPersonalizationSummary.Add($"Ingrediente desconhecido (ID: {ingredientId}) ignorado.");
                                }
                            }
                        }
                    }
                }

                // Atualiza o preço e o resumo do item no carrinho (APENAS PARA PRODUTOS INDIVIDUAIS)
                item.Price = recalculatedPrice;
                item.PersonalizationSummary = tempPersonalizationSummary.Any() ?
                                              "(" + string.Join(", ", tempPersonalizationSummary) + ")" : "";
            }

            cart.RemoveAll(item => itemsToRemove.Contains(item));
            HttpContext.Session.SetObject("Cart", cart);

            return View("Cart", cart);
        }

        [HttpPost]
        public IActionResult AddItem(Guid cartItemId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.CartItemId == cartItemId);

            if (item != null)
            {
                item.Quantity++;
                HttpContext.Session.SetObject("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DecreaseItem(Guid cartItemId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.CartItemId == cartItemId);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.Remove(item);
                }
                HttpContext.Session.SetObject("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveItem(Guid cartItemId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.CartItemId == cartItemId);
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