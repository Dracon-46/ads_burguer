using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Adicionar para usar .Include
using TotemPWA.Data;
using TotemPWA.Utilities;
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
        public async Task<IActionResult> Index() // Tornar a ação assíncrona
        {
            // Recupera o carrinho da sessão. Se não existir, cria uma nova lista vazia.
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            // Lista para armazenar itens que precisam ser removidos se o produto base não for encontrado
            var itemsToRemove = new List<CartItemViewModel>();

            // Recalcular o preço e o resumo para cada item no carrinho
            foreach (var item in cart)
            {
                // Inclui as propriedades necessárias para o cálculo do preço e resumo
                var product = await _context.Products
                                    .Include(p => p.Additionals!) // Inclui os 'Additionals' do produto
                                        .ThenInclude(pa => pa.Ingredient) // Em seguida, inclui o 'Ingredient' de cada 'Additional'
                                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                {
                    // Se o produto base não for encontrado no banco de dados, marca para remoção
                    itemsToRemove.Add(item);
                    continue; // Pula para o próximo item do carrinho
                }

                // Inicia o preço recalculado com o preço base do produto
                decimal recalculatedPrice = product.Price;
                var tempPersonalizationSummary = new List<string>();

                foreach (var entry in item.ManipulatedIngredientsWithQuantity)
                {
                    var ingredientId = entry.Key;
                    var finalQuantity = entry.Value;

                    // Tenta encontrar o 'Additional' correspondente no produto carregado.
                    // Isso é crucial para acessar as propriedades IsDefault e o preço do Ingredient.
                    var productAdditional = product.Additionals?.FirstOrDefault(a => a.IngredientId == ingredientId);

                    // Verifica se o ingrediente existe no produto ou se foi um erro de dado.
                    if (productAdditional == null || productAdditional.Ingredient == null)
                    {
                        // Se o ingrediente não faz parte do produto ou não tem dados, ignora ou loga.
                        if (finalQuantity > 0)
                        {
                            tempPersonalizationSummary.Add($"Erro: Ingrediente ID {ingredientId} inválido");
                        }
                        continue;
                    }

                    if (productAdditional.IsDefault)
                    {
                        // Adiciona ao resumo se a quantidade for > 0
                        if (finalQuantity > 0)
                        {
                            tempPersonalizationSummary.Add($"{finalQuantity}x {productAdditional.Ingredient.Name}");
                        } else {
                            // Se a quantidade final for 0 para um ingrediente padrão, significa que ele foi removido.
                            tempPersonalizationSummary.Add($"sem {productAdditional.Ingredient.Name}");
                        }

                        // Se a quantidade final é maior que 1, cobre as unidades adicionais
                        if (finalQuantity > 1)
                        {
                            recalculatedPrice += productAdditional.Ingredient.Price * (finalQuantity - 1);
                        }
                    }
                    else // Se o ingrediente NÃO é padrão (é um "extra" adicionado pelo usuário)
                    {
                        // Todas as unidades são cobradas.
                        if (finalQuantity > 0)
                        {
                            recalculatedPrice += productAdditional.Ingredient.Price * finalQuantity;
                            tempPersonalizationSummary.Add($"{finalQuantity}x {productAdditional.Ingredient.Name}");
                        }
                        // Se finalQuantity for 0 para um não-padrão, ele simplesmente não é adicionado ao preço nem ao resumo.
                    }
                }
                
                // Atualiza o preço e o resumo do item no carrinho
                item.Price = recalculatedPrice;
                item.PersonalizationSummary = tempPersonalizationSummary.Any() ?
                                              "(" + string.Join(", ", tempPersonalizationSummary) + ")" : "";
            }

            // Remove quaisquer itens de carrinho para os quais o produto base não foi encontrado
            cart.RemoveAll(item => itemsToRemove.Contains(item));

            // Salva o carrinho atualizado de volta na sessão
            HttpContext.Session.SetObject("Cart", cart);

            return View("Cart", cart);
        }

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