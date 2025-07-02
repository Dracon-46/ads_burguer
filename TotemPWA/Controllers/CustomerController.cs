// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels;
using TotemPWA.ViewModels;
using TotemPWA.Utilities; // Certifique-se de que esta classe de extensão para Session está disponível
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

            // Inicializamos com o preço base do produto.
            // A ideia é que o preço base já contemple a "quantidade padrão" dos ingredientes.
            decimal personalizedPrice = product.Price;
            var personalizationSummary = new List<string>();
            var manipulatedIngredientsWithQuantity = new Dictionary<int, int>();

            // Obtém as adições padrão do produto configuradas pelo administrador
            // Key: IngredientId, Value: Additional
            var productStandardAdditions = product.Additionals?.ToDictionary(a => a.IngredientId) ?? new Dictionary<int, Additional>();

            // Itera sobre os ingredientes manipulados enviados do formulário do cliente
            foreach (var entry in model.IngredientesManipuladasQuantidades)
            {
                var ingredientId = entry.Key;
                var finalQuantity = entry.Value; // Quantidade final desejada pelo cliente

                // Tenta encontrar o ingrediente correspondente no produto (configuração padrão)
                if (productStandardAdditions.TryGetValue(ingredientId, out var additional) && additional.Ingredient != null)
                {
                    var standardQuantity = additional.Quantity; // Quantidade padrão definida pelo admin para este produto
                    var ingredientPrice = additional.Ingredient.Price; // Preço do ingrediente (do modelo Ingredient)

                    // Lógica para limitar a quantidade adicionada pelo cliente
                    if (finalQuantity > additional.Ingredient.Limit)
                    {
                        finalQuantity = additional.Ingredient.Limit;
                    }
                    // Garante que a quantidade não seja negativa
                    if (finalQuantity < 0)
                    {
                        finalQuantity = 0;
                    }

                    // --- CALCULA O IMPACTO NO PREÇO E ATUALIZA O RESUMO ---

                    // Calcula o custo extra APENAS se a quantidade final for MAIOR que a quantidade padrão
                    if (finalQuantity > standardQuantity)
                    {
                        personalizedPrice += ingredientPrice * (finalQuantity - standardQuantity);
                        personalizationSummary.Add($"{finalQuantity}x {additional.Ingredient.Name}");
                    }
                    // Caso o cliente tenha REMOVIDO ingredientes que viriam por padrão
                    else if (finalQuantity < standardQuantity)
                    {
                        personalizationSummary.Add($"removido {standardQuantity - finalQuantity}x de {additional.Ingredient.Name}");
                        // Preço NÃO é reduzido aqui, pois o preço base do produto já contempla o custo padrão.
                        // Se você quiser reembolsar, a lógica seria: personalizedPrice -= ingredientPrice * (standardQuantity - finalQuantity);
                    }
                    // Caso a quantidade seja a mesma que a padrão e seja > 0, apenas adiciona ao resumo
                    else if (finalQuantity == standardQuantity && finalQuantity > 0)
                    {
                        personalizationSummary.Add($"{finalQuantity}x {additional.Ingredient.Name}");
                    }
                    // Caso finalQuantity seja 0 e standardQuantity seja 0, nada acontece ou adiciona "sem X"
                    else if (finalQuantity == 0 && standardQuantity == 0)
                    {
                         // Não faz nada ou adiciona se for o caso de um ingrediente que existia mas foi zerado.
                         // personalizationSummary.Add($"sem {additional.Ingredient.Name}");
                    }

                    // Armazena a quantidade final manipulada.
                    manipulatedIngredientsWithQuantity[ingredientId] = finalQuantity;
                }
                else
                {
                    // Cenário: Ingrediente enviado pelo formulário mas NÃO está na lista Product.Additionals
                    // Isso pode ser um bug ou tentativa de adicionar um ingrediente não associado ao produto.
                    // Para a sua nova lógica, apenas ingredientes que o admin "liberou" para o produto
                    // (estão em Product.Additionals) devem ser considerados.
                    // Se você QUISER permitir que o cliente adicione *qualquer* ingrediente do sistema como extra,
                    // mesmo que o admin não o tenha associado ao produto, você teria que buscá-lo do _context.Ingredients
                    // e adicionar o preço. Por enquanto, a lógica assume que só se manipula ingredientes "pré-aprovados".
                    if (finalQuantity > 0)
                    {
                        var unknownIngredient = await _context.Ingredients.FindAsync(ingredientId);
                        if (unknownIngredient != null)
                        {
                            // AQUI: Se você quer que ingredientes não padrão sejam cobrados como extras
                            personalizedPrice += unknownIngredient.Price * finalQuantity;
                            personalizationSummary.Add($"extra {finalQuantity}x {unknownIngredient.Name}");
                            manipulatedIngredientsWithQuantity[ingredientId] = finalQuantity;
                        }
                        else
                        {
                            personalizationSummary.Add($"Ingrediente desconhecido (ID: {ingredientId}) ignorado.");
                        }
                    }
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
                    Price = personalizedPrice, // Preço já com personalizações
                    Image = product.Image ?? product.ImageUrl ?? "/images/products/default_product.png",
                    Quantity = 1, // Geralmente um item personalizado é adicionado com quantidade 1
                    CartItemId = Guid.NewGuid(),
                    ManipulatedIngredientsWithQuantity = manipulatedIngredientsWithQuantity, // SALVA O DICIONÁRIO COMPLETO
                    PersonalizationSummary = summaryText,
                });
            }
            else // Editando item existente no carrinho
            {
                cartItemToModify.Price = personalizedPrice;
                cartItemToModify.ManipulatedIngredientsWithQuantity = manipulatedIngredientsWithQuantity; // ATUALIZA O DICIONÁRIO COMPLETO
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
                                .Include(p => p.Additionals!) // Inclua todos os Additionals
                                .ThenInclude(pa => pa.Ingredient) // E os Ingredientes relacionados
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

        // --- Ações abaixo são do CustomerController para TelaProduto ---

       
    }
}