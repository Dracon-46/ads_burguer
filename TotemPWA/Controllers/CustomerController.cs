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
        public async Task<IActionResult> SalvarPersonalizacaoCombo(PersonalizarComboInputModel model)
        {
            
            // 1. Encontre o produto combo principal (apenas para obter o nome e preço base do combo)
            var comboProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == model.ComboProductId);

            if (comboProduct == null) return NotFound("Combo não encontrado.");

            decimal totalPersonalizedPrice = comboProduct.Price; // Preço inicial do combo
            var overallPersonalizationSummary = new List<string>();
            
            // Para armazenar as personalizações de cada item do combo no CartItemViewModel
            var comboItemsManipulatedIngredients = new Dictionary<int, Dictionary<int, int>>(); 

            // Itera sobre cada item do combo que foi personalizado no formulário
            foreach (var itemPersonalization in model.ItemPersonalizations)
            {
                var itemProductId = itemPersonalization.ProductId;
                var itemProduct = await _context.Products
                    .Include(p => p.Additionals!)
                        .ThenInclude(pa => pa.Ingredient)
                    .FirstOrDefaultAsync(p => p.Id == itemProductId);

                if (itemProduct == null)
                {
                    overallPersonalizationSummary.Add($"Item '{itemProductId}' do combo não encontrado. Personalização ignorada.");
                    continue; // Pule para o próximo item
                }

                // Obtém as adições padrão do item do combo
                var itemStandardAdditions = itemProduct.Additionals?.ToDictionary(a => a.IngredientId) ?? new Dictionary<int, Additional>();
                var itemManipulatedIngredients = new Dictionary<int, int>(); // Para este item específico do combo
                var itemSummary = new List<string>();

                // Itera sobre os ingredientes manipulados para ESTE ITEM DO COMBO
                foreach (var entry in itemPersonalization.IngredientesManipuladasQuantidades)
                {
                    var ingredientId = entry.Key;
                    var finalQuantity = entry.Value;

                    if (itemStandardAdditions.TryGetValue(ingredientId, out var additional) && additional.Ingredient != null)
                    {
                        var standardQuantity = additional.Quantity;
                        var ingredientPrice = additional.Ingredient.Price;

                        // Aplica limites e garante não-negativo
                        if (finalQuantity > additional.Ingredient.Limit) finalQuantity = additional.Ingredient.Limit;
                        if (finalQuantity < 0) finalQuantity = 0;

                        // Calcula o custo extra apenas se a quantidade final for MAIOR que a quantidade padrão
                        if (finalQuantity > standardQuantity)
                        {
                            totalPersonalizedPrice += ingredientPrice * (finalQuantity - standardQuantity);
                            itemSummary.Add($"{finalQuantity}x {additional.Ingredient.Name}");
                        }
                        else if (finalQuantity < standardQuantity)
                        {
                            itemSummary.Add($"removido {standardQuantity - finalQuantity}x de {additional.Ingredient.Name}");
                            // Não reduz o preço aqui, como na lógica de produtos individuais.
                        }
                        else if (finalQuantity == standardQuantity && finalQuantity > 0)
                        {
                            itemSummary.Add($"{finalQuantity}x {additional.Ingredient.Name}");
                        }
                        
                        itemManipulatedIngredients[ingredientId] = finalQuantity; // Armazena a quantidade final manipulada para este item
                    }
                    else // Ingrediente não padrão para este item do combo
                    {
                        if (finalQuantity > 0)
                        {
                            var unknownIngredient = await _context.Ingredients.FindAsync(ingredientId);
                            if (unknownIngredient != null)
                            {
                                totalPersonalizedPrice += unknownIngredient.Price * finalQuantity;
                                itemSummary.Add($"extra {finalQuantity}x {unknownIngredient.Name}");
                                itemManipulatedIngredients[ingredientId] = finalQuantity;
                            }
                            else
                            {
                                itemSummary.Add($"Ingrediente desconhecido (ID: {ingredientId}) ignorado no item {itemProduct.Name}.");
                            }
                        }
                    }
                }
                // Adicione o resumo da personalização deste item ao resumo geral do combo
                if (itemSummary.Any())
                {
                    overallPersonalizationSummary.Add($"{itemProduct.Name}: ({string.Join(", ", itemSummary)})");
                    comboItemsManipulatedIngredients[itemProductId] = itemManipulatedIngredients; // Salva o dicionário por item
                }
            }

            var finalSummaryText = overallPersonalizationSummary.Any()
                ? "Combo " + comboProduct.Name + ": " + string.Join("; ", overallPersonalizationSummary)
                : "Combo " + comboProduct.Name + " (sem personalização)";


            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            CartItemViewModel? cartItemToModify = null;
            if (model.CartItemId != Guid.Empty)
            {
                cartItemToModify = cart.FirstOrDefault(ci => ci.CartItemId == model.CartItemId);
            }

            if (cartItemToModify == null) // Novo item de combo personalizado
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = comboProduct.Id, // ID do combo principal
                    Name = comboProduct.Name,
                    Price = totalPersonalizedPrice, // Preço total personalizado do combo
                    Image = comboProduct.Image ?? comboProduct.ImageUrl ?? "/images/products/default_product.png",
                    Quantity = 1,
                    CartItemId = Guid.NewGuid(),
                    // NOVO: ManipulatedIngredientsWithQuantity para combos precisa armazenar por ITEM DO COMBO
                    // Isso exigiria uma mudança na estrutura do CartItemViewModel se você precisa detalhar por item.
                    // Para simplificar, vou manter a estrutura plana do dicionário, mas você pode querer mudar isso.
                    // Se você realmente precisa de personalizações por sub-item no carrinho,
                    // o CartItemViewModel precisaria de List<SubItemPersonalization> com cada sub-item tendo seu Dictionary<int,int>.
                    // Por enquanto, vamos manter um dicionário global de todas as manipulações de todos os ingredientes,
                    // mas isso pode não ser ideal para relatórios detalhados ou reedição.
                    // Pelo que vejo no seu CartItemViewModel, você só tem um Dictionary<int, int> para todo o item do carrinho.
                    // Então, vamos consolidar todas as manipulações em um único dicionário para o CartItemViewModel.
                    // Isso significa que o dicionário de cada 'itemPersonalization' deve ser MERGULHADO em um dicionário global.
                    ManipulatedIngredientsWithQuantity = ConsolidateComboManipulations(comboItemsManipulatedIngredients),
                    PersonalizationSummary = finalSummaryText,
                });
            }
            else // Editando item de combo existente no carrinho
            {
                cartItemToModify.Price = totalPersonalizedPrice;
                cartItemToModify.ManipulatedIngredientsWithQuantity = ConsolidateComboManipulations(comboItemsManipulatedIngredients);
                cartItemToModify.PersonalizationSummary = finalSummaryText;
            }

            HttpContext.Session.SetObject("Cart", cart);
            TempData["Message"] = "Combo personalizado e adicionado/atualizado no carrinho!";
            return RedirectToAction("Index", "Cart");
        }

        // Helper method to consolidate all manipulated ingredients from combo items into a single dictionary
        private Dictionary<int, int> ConsolidateComboManipulations(Dictionary<int, Dictionary<int, int>> comboItemsManipulations)
        {
            var consolidated = new Dictionary<int, int>();
            foreach (var itemEntry in comboItemsManipulations)
            {
                foreach (var ingredientEntry in itemEntry.Value)
                {
                    // Se o ingrediente já existe, some as quantidades.
                    // Caso contrário, adicione-o.
                    if (consolidated.ContainsKey(ingredientEntry.Key))
                    {
                        consolidated[ingredientEntry.Key] += ingredientEntry.Value;
                    }
                    else
                    {
                        consolidated[ingredientEntry.Key] = ingredientEntry.Value;
                    }
                }
            }
            return consolidated;
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

            // 1. Inicializa o preço base com o preço do produto.
            decimal personalizedPrice = product.Price;

            // 2. Verifica e aplica promoções ativas.
            var activePromotion = await _context.Promotions
                .Where(p => p.ProductId == product.Id && p.ValidUntil >= DateTime.Today)
                .FirstOrDefaultAsync();

            if (activePromotion != null)
            {
                // Aplica o desconto ao preço base
                personalizedPrice = personalizedPrice * (1 - activePromotion.Percent / 100);
            }
            
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
            // Primeiro, verificar se é um combo
            var isCombo = await _context.Combos.AnyAsync(c => c.ProductComboId == productId);
            
            if (isCombo)
            {
                // Se for um combo, buscar o produto combo e seus itens
                var comboProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId);
                    
                if (comboProduct == null)
                    return NotFound("Produto combo não encontrado.");

                // Buscar todos os itens do combo, incluindo seus produtos e adicionais
                var comboItemsRaw = await _context.Combos
                    .Include(c => c.Product) // O produto real que é um item do combo
                        .ThenInclude(p => p.Additionals!) // Adicionais do item do combo
                            .ThenInclude(pa => pa.Ingredient) // Detalhes do ingrediente
                    .Where(c => c.ProductComboId == productId)
                    .ToListAsync();
                    
                // Mapear para PersonalizarComboViewModel e seus ComboItemViewModel
                var comboViewModel = new PersonalizarComboViewModel
                {
                    ComboProduct = comboProduct,
                    CartItemId = cartItemId ?? Guid.Empty,
                    TotalPrice = comboProduct.Price, // Preço base do combo (será ajustado pelo JS/backend)
                    ComboItems = comboItemsRaw.Select(ci => new ComboItemViewModel
                    {
                        ProductId = ci.Product!.Id,
                        ProductName = ci.Product.Name,
                        ProductPrice = ci.Product.Price,
                        ProductImageUrl = ci.Product.ImageUrl,
                        ProductAdditionals = ci.Product.Additionals, // Passa os adicionais do item
                        CanCustomize = true // Ou baseie isso em alguma propriedade do produto/additional
                    }).ToList()
                };

                // Se for edição, pré-preenche o ViewModel com as quantidades manipuladas do carrinho
                // Para combos, isso é mais complexo, pois ManipulatedIngredientsWithQuantity é plano.
                // Você precisará de uma estrutura mais complexa para armazenar as manipulações por PRODUTO DENTRO DO COMBO.
                // Por enquanto, vamos manter a estrutura plana para simplificar, mas saiba que pode ter limitações.
                if (cartItemId.HasValue && cartItemId.Value != Guid.Empty)
                {
                    var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
                    var existingCartItem = cart.FirstOrDefault(ci => ci.CartItemId == cartItemId.Value);
                    if (existingCartItem != null)
                    {
                        // ATENÇÃO: Se as manipulações forem globais para o combo, isso funciona.
                        // Se forem por item dentro do combo, o CartItemViewModel precisaria de uma estrutura mais complexa.
                        comboViewModel.QuantidadesManipuladas = existingCartItem.ManipulatedIngredientsWithQuantity ?? new Dictionary<int, int>();
                    }
                }
                
                return View("~/Views/Home/PersonalizarCombo.cshtml", comboViewModel);
            }
            else
            {
                // Se não for combo, continuar com o fluxo normal para produtos individuais
                var product = await _context.Products
                                    .Include(p => p.Additionals!)
                                    .ThenInclude(pa => pa.Ingredient)
                                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                    return NotFound("Produto não encontrado.");

                // >>> Adicionar lógica de Promoção aqui <<<
                decimal basePrice = product.Price;

                var activePromotion = await _context.Promotions
                    .Where(p => p.ProductId == productId && p.ValidUntil >= DateTime.Today)
                    .FirstOrDefaultAsync();

                if (activePromotion != null)
                {
                    // Calcula o preço com desconto (Ex: Price * (1 - Percent/100))
                    basePrice = basePrice * (1 - activePromotion.Percent / 100);
                }

                var viewModel = new PersonalizarProdutoViewModel
                {
                    Produto = product,
                    CartItemId = cartItemId ?? Guid.Empty,
                    ProdutoAdditionals = product.Additionals,
                    BasePriceWithPromotion = basePrice // Adicione esta linha
                };
                // Se for edição, pré-preenche o ViewModel com as quantidades manipuladas do carrinho
               if (cartItemId.HasValue && cartItemId.Value != Guid.Empty)
                {
                    var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
                    var existingCartItem = cart.FirstOrDefault(ci => ci.CartItemId == cartItemId.Value);
                    if (existingCartItem != null)
                    {
                        viewModel.QuantidadesManipuladas = existingCartItem.ManipulatedIngredientsWithQuantity ?? new Dictionary<int, int>();
                    }
                }

                return View("~/Views/Home/PersonalizarProdutos.cshtml", viewModel);
            }
        }

        
    }
}