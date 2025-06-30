// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels;
using TotemPWA.ViewModels;
using TotemPWA.Utilities; // Certifique-se de que SessionExtensions está aqui
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
                                .Include(p => p.Category)
                                    .ThenInclude(c => c.ParentCategory) // Inclua a categoria pai
                                .Include(p => p.Additionals!)
                                    .ThenInclude(pa => pa.Ingredient)
                                .FirstOrDefaultAsync(p => p.Id == model.ProdutoId);

            if (product == null) return NotFound("Produto não encontrado.");

            decimal personalizedPrice = product.Price;
            var personalizationSummary = new List<string>();

            var productAdditionals = product.Additionals ?? new List<Additional>();
            var defaultIngredients = productAdditionals.Where(pa => pa.IsDefault).Select(pa => pa.Ingredient!).ToList();
            var addableIngredients = productAdditionals.Where(pa => pa.CanBeAdded).ToList();


            // Processar ingredientes REMOVIDOS do padrão
            // Mantenha a mesma lógica para remoção, pois se o usuário clicou em remover, ele não quer aquele ingrediente padrão.
            foreach (var removedId in model.IngredientesParaRemover)
            {
                var ingredientToRemove = defaultIngredients.FirstOrDefault(i => i.Id == removedId);
                var additionalEntry = productAdditionals.FirstOrDefault(a => a.IngredientId == removedId && a.IsDefault && a.CanBeRemoved);

                if (ingredientToRemove != null && additionalEntry != null)
                {
                    personalizationSummary.Add($"sem {ingredientToRemove.Name}");
                }
            }

            // Processar ingredientes ADICIONADOS (Agora pode ser múltiplos do mesmo)
            // O inputModel.IngredientesParaAdicionar deve ter os IDs de todos os adicionados,
            // mesmo que repetidos para indicar múltiplas unidades.
            foreach (var addedId in model.IngredientesParaAdicionar)
            {
                var additionalToAdd = addableIngredients.FirstOrDefault(a => a.IngredientId == addedId);
                if (additionalToAdd != null)
                {
                    personalizedPrice += additionalToAdd.Price; // Adiciona o custo do ingrediente extra
                    personalizationSummary.Add($"com {additionalToAdd.Ingredient!.Name}");
                }
            }

            // Processar tamanho selecionado
            if (!string.IsNullOrEmpty(model.TamanhoSelecionado))
            {
                // Lógica de preço por tamanho. Ajuste os nomes das categorias aqui conforme seu DB.
                string mainCategoryName = product.Category?.ParentCategory?.Name?.ToLower() ?? product.Category?.Name?.ToLower() ?? "";
                
                if (mainCategoryName == "bebidas" || mainCategoryName == "acompanhamentos")
                {
                    if (model.TamanhoSelecionado.ToLower() == "grande") personalizedPrice += 2.00M;
                    else if (model.TamanhoSelecionado.ToLower() == "família") personalizedPrice += 5.00M;
                }
                personalizationSummary.Add($"tamanho {model.TamanhoSelecionado}");
            }

            var summaryText = personalizationSummary.Any() ? "(" + string.Join(", ", personalizationSummary) + ")" : "";

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
                    SelectedSize = model.TamanhoSelecionado,
                    AddedIngredientIds = model.IngredientesParaAdicionar, // Guarda todos os IDs adicionados
                    RemovedIngredientIds = model.IngredientesParaRemover, // Guarda todos os IDs removidos
                    PersonalizationSummary = summaryText
                });
            }
            else // Editando item existente no carrinho
            {
                cartItemToModify.Price = personalizedPrice;
                cartItemToModify.SelectedSize = model.TamanhoSelecionado;
                cartItemToModify.AddedIngredientIds = model.IngredientesParaAdicionar;
                cartItemToModify.RemovedIngredientIds = model.IngredientesParaRemover;
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
                                .Include(p => p.Category)
                                    .ThenInclude(c => c.ParentCategory) // Carrega a categoria pai
                                .Include(p => p.Additionals!)
                                    .ThenInclude(pa => pa.Ingredient)
                                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound("Produto não encontrado.");

            // Determina o tipo do produto usando a categoria PAI se existir, senão a própria categoria
            string mainCategoryName = product.Category?.ParentCategory?.Name?.ToLower() ?? product.Category?.Name?.ToLower() ?? "outro";

            var viewModel = new PersonalizarProdutoViewModel
            {
                Produto = product,
                TipoProduto = mainCategoryName, // Usa o nome da categoria principal para definir o "tipo"
                CartItemId = cartItemId ?? Guid.Empty
            };

            // Popula IngredientesDisponiveis (ingredientes que podem ser adicionados para ESTE PRODUTO)
            viewModel.IngredientesDisponiveis = product.Additionals!
                                                    .Where(a => a.CanBeAdded) // Apenas os que podem ser adicionados para este produto
                                                    .Select(a => a.Ingredient!)
                                                    .ToList();

            // Popula IngredientesPadrao (ingredientes que vêm com ESTE PRODUTO por padrão)
            viewModel.IngredientesPadrao = product.Additionals!
                                                .Where(pa => pa.IsDefault)
                                                .Select(pa => pa.Ingredient!)
                                                .ToList();

            // Popula TamanhosDisponiveis com base na categoria principal
            viewModel.TamanhosDisponiveis = GetTamanhosParaCategoria(mainCategoryName);

            // Se for edição, pré-preenche o ViewModel com as personalizações atuais do carrinho
            if (cartItemId.HasValue && cartItemId.Value != Guid.Empty)
            {
                var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
                var existingCartItem = cart.FirstOrDefault(ci => ci.CartItemId == cartItemId.Value);
                if (existingCartItem != null)
                {
                    viewModel.TamanhoAtual = existingCartItem.SelectedSize;
                    viewModel.IngredientesAtuaisAdicionados = existingCartItem.AddedIngredientIds;
                    viewModel.IngredientesAtuaisRemovidos = existingCartItem.RemovedIngredientIds;
                }
            }

            return View("~/Views/Home/PersonalizarProdutos.cshtml", viewModel);
        }

        // Função auxiliar para obter tamanhos por categoria
        private List<string> GetTamanhosParaCategoria(string? categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return new List<string>();

            switch (categoryName.ToLower())
            {
                case "bebidas":
                    return new List<string> { "Pequeno", "Médio", "Grande" };
                case "acompanhamentos":
                    return new List<string> { "Pequeno", "Médio", "Grande", "Família" };
                // Adicione outras categorias que precisam de tamanhos específicos
                default:
                    return new List<string>();
            }
        }
    }
}