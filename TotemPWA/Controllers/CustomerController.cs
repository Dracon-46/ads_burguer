using Microsoft.AspNetCore.Mvc;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels; // Garanta que este using está presente
using TotemPWA.ViewModels; // Garanta que este using está presente
using Microsoft.EntityFrameworkCore;
using TotemPWA.Utilities;
using System; // Adicione este using para Guid
using System.Linq; // Para FirstOrDefault
using System.Collections.Generic; // Para List

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
            var product = await _context.Products // Inclua category e additionals aqui
                                .Include(p => p.Category)
                                .Include(p => p.Additionals!)
                                    .ThenInclude(pa => pa.Ingredient)
                                .FirstOrDefaultAsync(p => p.Id == model.ProdutoId); // Usa ProdutoId do inputModel

            if (product == null) return NotFound("Produto não encontrado.");

            decimal personalizedPrice = product.Price;
            var personalizationSummary = new List<string>();

            var productAdditionals = product.Additionals ?? new List<Additional>();
            var defaultIngredients = productAdditionals.Where(pa => pa.IsDefault).Select(pa => pa.Ingredient!).ToList();
            var addableIngredients = productAdditionals.Where(pa => pa.CanBeAdded).ToList(); // Mantenha o Additional completo para pegar o Price

            // Processar ingredientes REMOVIDOS do padrão
            foreach (var removedId in model.IngredientesParaRemover)
            {
                var ingredientToRemove = defaultIngredients.FirstOrDefault(i => i.Id == removedId);
                var additionalEntry = productAdditionals.FirstOrDefault(a => a.IngredientId == removedId && a.IsDefault && a.CanBeRemoved);

                if (ingredientToRemove != null && additionalEntry != null)
                {
                    personalizationSummary.Add($"sem {ingredientToRemove.Name}");
                }
            }

            // Processar ingredientes ADICIONADOS
            foreach (var addedId in model.IngredientesParaAdicionar)
            {
                var additionalToAdd = addableIngredients.FirstOrDefault(a => a.IngredientId == addedId); // Busca o Additional para pegar o Price
                if (additionalToAdd != null)
                {
                    personalizedPrice += additionalToAdd.Price; // Adiciona o custo do ingrediente extra
                    personalizationSummary.Add($"com {additionalToAdd.Ingredient!.Name}");
                }
            }

            // Processar tamanho selecionado
            if (!string.IsNullOrEmpty(model.TamanhoSelecionado))
            {
                // Lógica de preço por tamanho
                if (product.Category?.Name?.ToLower() == "bebidas" || product.Category?.Name?.ToLower() == "acompanhamentos")
                {
                    if (model.TamanhoSelecionado.ToLower() == "grande") personalizedPrice += 2.00M;
                    else if (model.TamanhoSelecionado.ToLower() == "família") personalizedPrice += 5.00M;
                }
                personalizationSummary.Add($"tamanho {model.TamanhoSelecionado}");
            }

            // Gerar o resumo final da personalização
            var summaryText = personalizationSummary.Any() ? "(" + string.Join(", ", personalizationSummary) + ")" : "";

            // Lógica para adicionar/atualizar no CARRINHO (SESSÃO)
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            CartItemViewModel? cartItemToModify = null;
            if (model.CartItemId != Guid.Empty) // model.CartItemId já é Guid
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
                    CartItemId = Guid.NewGuid(), // Novo GUID para esta instância única
                    SelectedSize = model.TamanhoSelecionado,
                    AddedIngredientIds = model.IngredientesParaAdicionar,
                    RemovedIngredientIds = model.IngredientesParaRemover,
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

            HttpContext.Session.SetObject("Cart", cart); // Salva o carrinho na sessão
            TempData["Message"] = "Produto personalizado e adicionado/atualizado no carrinho!";
            return RedirectToAction("Index", "Cart");
        }


        [HttpGet]
        public async Task<IActionResult> PersonalizarProdutos(int productId, Guid? cartItemId) // Mude orderItemId para productId e adicione cartItemId
        {
            var product = await _context.Products // Carrega o produto para a tela de personalização
                                .Include(p => p.Category)
                                .Include(p => p.Additionals!) // Carrega os Additionals para saber os ingredientes padrão/adicionáveis
                                    .ThenInclude(pa => pa.Ingredient)
                                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound("Produto não encontrado.");

            // Identifica o tipo do produto pela Category.Name, que é mais robusto
            string tipo = product.Category?.Name?.ToLower() ?? "lanche"; // Padrão "lanche" se categoria for nula

            var viewModel = new PersonalizarProdutoViewModel
            {
                Produto = product,
                TipoProduto = tipo,
                CartItemId = cartItemId ?? Guid.Empty // Passa o GUID para a view
            };

            // Popula IngredientesDisponiveis (ingredientes que podem ser adicionados)
            viewModel.IngredientesDisponiveis = await _context.Additionals
                                                    .Where(a => a.ProductId == product.Id && a.CanBeAdded)
                                                    .Select(a => a.Ingredient!)
                                                    .ToListAsync();

            // Popula IngredientesPadrao (ingredientes que vêm com o produto por padrão)
            viewModel.IngredientesPadrao = product.Additionals!
                                                .Where(pa => pa.IsDefault)
                                                .Select(pa => pa.Ingredient!)
                                                .ToList();

            // Popula TamanhosDisponiveis com base na categoria
            viewModel.TamanhosDisponiveis = GetTamanhosParaCategoria(tipo);

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
            // Use o nome da sua view de personalização, que pelo que vi é "PersonalizarProdutos"
            return View("~/Views/Home/PersonalizarProdutos.cshtml", viewModel); // <<-- CORRIGIDO AQUI!
        }

        // Função auxiliar para obter tamanhos por categoria (coloque-a aqui ou em um helper)
        private List<string> GetTamanhosParaCategoria(string? categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return new List<string>();

            switch (categoryName.ToLower())
            {
                case "bebidas": // Use o nome da categoria que você tem no banco de dados (plural ou singular)
                    return new List<string> { "Pequeno", "Médio", "Grande" };
                case "acompanhamentos": // Use o nome da categoria que você tem no banco de dados
                    return new List<string> { "Pequeno", "Médio", "Grande", "Família" };
                default:
                    return new List<string>();
            }
        }
    }
}