// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels;
using TotemPWA.ViewModels;
using TotemPWA.Utilities; // Certifique-se de que SessionExtensions está aqui
using System.Linq; // Adicione este using
using System.Collections.Generic; // Adicione este using
using System; // Adicione este using para Guid

namespace TotemPWA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TelaCPF()
        {
            return View();
        }

        public IActionResult TelaNome()
        {
            return View();
        }

        public IActionResult TelaHome_Crud()
        {
            return View();
        }

        public IActionResult SelecionarPedido()
        {
            return View();
        }

        public IActionResult Cupom()
        {
            return View();
        }

        public IActionResult TelaFinal()
        {
            return View();
        }

        public IActionResult PersonalizarProdutos()
        {
            return View();
        }

        public IActionResult TelaPersoCombo()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("TelaProduto/{categorySlug?}/{subcategorySlug?}")]
        public async Task<IActionResult> TelaProduto(string categorySlug, string subcategorySlug = null)
        {
            // 1. Encontrar a categoria ativa usando o SLUG
            var rootCategoriesRaw = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();

            Category activeCategory;

            if (!string.IsNullOrEmpty(categorySlug))
            {
                // Se um slug foi passado na URL, encontre a categoria correspondente
                activeCategory = rootCategoriesRaw.FirstOrDefault(c => c.Slug == categorySlug);
            }
            else
            {
                // Se nenhum slug foi passado, pegue a primeira categoria como padrão
                activeCategory = rootCategoriesRaw.FirstOrDefault();
            }

            // Se nenhuma categoria for encontrada, pode ser bom tratar o erro (ex: return NotFound();)
            if (activeCategory == null) return NotFound("Categoria não encontrada.");

            // Pega o ID da categoria ativa para usar nas próximas consultas
            var activeCategoryId = activeCategory.Id;

            var rootCategories = rootCategoriesRaw
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    slug = c.Slug,
                    active = c.Id == activeCategoryId
                })
                .ToList();

            // 2. Encontrar a subcategoria ativa usando o SLUG
            var subcategoriesRaw = await _context.Categories
                .Where(c => c.ParentCategoryId == activeCategoryId)
                .ToListAsync();

            Category activeSubcategory;

            if (!string.IsNullOrEmpty(subcategorySlug))
            {
                // Se um slug de subcategoria foi passado, encontre-o
                activeSubcategory = subcategoriesRaw.FirstOrDefault(s => s.Slug == subcategorySlug);
            }
            else
            {
                // Senão, pegue a primeira subcategoria como padrão
                activeSubcategory = subcategoriesRaw.FirstOrDefault();
            }

            // O ID da subcategoria ativa (pode ser nulo se não houver subcategorias)
            var activeSubcategoryId = activeSubcategory?.Id;

            var subcategories = subcategoriesRaw
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    slug = c.Slug,
                    active = c.Id == activeSubcategoryId
                })
                .ToList();

            // 3. Buscar produtos com base no ID da subcategoria ativa
            var products = new List<object>();
            if (activeSubcategoryId != null)
            {
                products = await _context.Products
                    .Where(p => p.CategoryId == activeSubcategoryId)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price,
                        image = p.Image
                    })
                    .ToListAsync<object>();
            }

            // Obter o carrinho da sessão
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            // Passar os dados para a ViewBag
            ViewBag.CategorySlug = activeCategory.Slug;
            ViewBag.Categories = rootCategories;
            ViewBag.SubCategories = subcategories;
            ViewBag.Products = products;

            // Passar o carrinho como modelo para a view
            return View(cart);
        }

        // NOVO: Ação Personalizar (GET) para carregar os dados para a view
        [HttpGet]
        public async Task<IActionResult> Personalizar(int productId, Guid? cartItemId) // cartItemId é opcional para edição
        {
            var product = await _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.Additionals!) // Carrega os Additionals associados ao produto
                                    .ThenInclude(pa => pa.Ingredient) // E os Ingredients dentro dos Additionals
                                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            // Preenche o ViewModel de Personalização
            var viewModel = new PersonalizarProdutoViewModel
            {
                Produto = product,
                TipoProduto = product.Category?.Name ?? "Outro", // Garante que TipoProduto não seja nulo
                CartItemId = cartItemId ?? Guid.Empty // Passa o GUID, ou um GUID vazio se for um novo item
            };

            // Popula IngredientesDisponiveis (todos os que podem ser adicionados)
            viewModel.IngredientesDisponiveis = await _context.Additionals
                                                    .Where(a => a.CanBeAdded) // Apenas ingredientes que podem ser adicionados
                                                    .Select(a => a.Ingredient!) // Seleciona o Ingredient em si
                                                    .Distinct() // Evita duplicatas se um ingrediente puder ser adicionado a vários produtos
                                                    .ToListAsync();

            // Popula IngredientesPadrao (os que vêm com o produto por padrão)
            viewModel.IngredientesPadrao = product.Additionals!
                                                .Where(pa => pa.IsDefault)
                                                .Select(pa => pa.Ingredient!)
                                                .ToList();

            // Popula TamanhosDisponiveis com base na categoria
            viewModel.TamanhosDisponiveis = GetTamanhosParaCategoria(viewModel.TipoProduto);

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

            return View("PersonalizarProdutos", viewModel);
        }

        // NOVO: Ação SalvarPersonalizacao (POST) para processar os dados do formulário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarPersonalizacao(PersonalizarProdutoInputModel inputModel)
        {
            // Validação básica do modelo
            if (!ModelState.IsValid)
            {
                // Se o modelo for inválido, você pode recarregar a tela de personalização com erros
                // Para simplificar, estamos apenas redirecionando, mas em um cenário real, você
                // recarregaria a view PersonalizarProdutos com o inputModel e erros.
                TempData["ErrorMessage"] = "Erro nos dados de personalização. Tente novamente.";
                return RedirectToAction("Personalizar", new { productId = inputModel.ProdutoId, cartItemId = inputModel.CartItemId });
            }

            var product = await _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.Additionals!)
                                    .ThenInclude(pa => pa.Ingredient)
                                .FirstOrDefaultAsync(p => p.Id == inputModel.ProdutoId);

            if (product == null) return NotFound("Produto não encontrado.");

            decimal personalizedPrice = product.Price;
            var personalizationSummary = new List<string>();

            // Obter todos os ingredientes disponíveis e padrão para o produto
            var productAdditionals = product.Additionals ?? new List<Additional>();
            var defaultIngredients = productAdditionals.Where(pa => pa.IsDefault).Select(pa => pa.Ingredient!).ToList();
            var addableIngredients = productAdditionals.Where(pa => pa.CanBeAdded).Select(pa => pa).ToList();


            // 1. Processar ingredientes REMOVIDOS do padrão
            foreach (var removedId in inputModel.IngredientesParaRemover)
            {
                var ingredientToRemove = defaultIngredients.FirstOrDefault(i => i.Id == removedId);
                var additionalEntry = productAdditionals.FirstOrDefault(a => a.IngredientId == removedId && a.IsDefault);

                if (ingredientToRemove != null && additionalEntry != null && additionalEntry.CanBeRemoved)
                {
                    // Remover não altera o preço base em lanches, apenas o que vem no produto.
                    // Se a lógica fosse que remover algo caro diminui o preço, seria aqui.
                    personalizationSummary.Add($"sem {ingredientToRemove.Name}");
                }
            }

            // 2. Processar ingredientes ADICIONADOS
            foreach (var addedId in inputModel.IngredientesParaAdicionar)
            {
                var ingredientToAdd = addableIngredients.FirstOrDefault(a => a.IngredientId == addedId);
                if (ingredientToAdd != null)
                {
                    personalizedPrice += ingredientToAdd.Price; // Adiciona o custo do ingrediente extra
                    personalizationSummary.Add($"com {ingredientToAdd.Ingredient!.Name}");
                }
            }

            // 3. Processar tamanho selecionado (se aplicável)
            if (!string.IsNullOrEmpty(inputModel.TamanhoSelecionado))
            {
                // A lógica de preço por tamanho deve ser definida aqui.
                // Exemplo: se o tamanho "Grande" de uma bebida custa mais
                if (product.Category?.Name?.ToLower() == "bebida" || product.Category?.Name?.ToLower() == "acompanhamento")
                {
                    // Adapte esta lógica conforme a sua tabela de preços por tamanho, se houver
                    if (inputModel.TamanhoSelecionado.ToLower() == "grande")
                    {
                        personalizedPrice += 2.00M; // Exemplo de acréscimo de preço para "Grande"
                    }
                    else if (inputModel.TamanhoSelecionado.ToLower() == "família")
                    {
                        personalizedPrice += 5.00M; // Exemplo para "Família"
                    }
                }
                personalizationSummary.Add($"tamanho {inputModel.TamanhoSelecionado}");
            }


            // Gerar o resumo final da personalização
            var summaryText = personalizationSummary.Any() ? "(" + string.Join(", ", personalizationSummary) + ")" : "";

            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            CartItemViewModel? cartItemToModify = null;
            if (inputModel.CartItemId != Guid.Empty)
            {
                cartItemToModify = cart.FirstOrDefault(ci => ci.CartItemId == inputModel.CartItemId);
            }

            if (cartItemToModify == null) // Novo item personalizado
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = personalizedPrice, // Preço já com personalizações
                    Image = product.Image ?? product.ImageUrl ?? "/images/products/default_product.png", // Imagem padrão
                    Quantity = 1,
                    CartItemId = Guid.NewGuid(), // Novo GUID para identificar esta personalização única
                    SelectedSize = inputModel.TamanhoSelecionado,
                    AddedIngredientIds = inputModel.IngredientesParaAdicionar,
                    RemovedIngredientIds = inputModel.IngredientesParaRemover,
                    PersonalizationSummary = summaryText
                });
            }
            else // Editando item existente
            {
                cartItemToModify.Price = personalizedPrice;
                cartItemToModify.SelectedSize = inputModel.TamanhoSelecionado;
                cartItemToModify.AddedIngredientIds = inputModel.IngredientesParaAdicionar;
                cartItemToModify.RemovedIngredientIds = inputModel.IngredientesParaRemover;
                cartItemToModify.PersonalizationSummary = summaryText;
            }

            HttpContext.Session.SetObject("Cart", cart);
            TempData["Message"] = "Produto personalizado e adicionado/atualizado no carrinho!";
            return RedirectToAction("Index", "Cart"); // Redireciona para o carrinho
        }

        // Função auxiliar para obter tamanhos por categoria
        private List<string> GetTamanhosParaCategoria(string? categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return new List<string>();

            switch (categoryName.ToLower())
            {
                case "bebidas": // Use o nome da categoria que você tem no banco de dados
                    return new List<string> { "Pequeno", "Médio", "Grande" };
                case "acompanhamentos": // Use o nome da categoria que você tem no banco de dados
                    return new List<string> { "Pequeno", "Médio", "Grande", "Família" };
                default:
                    return new List<string>();
            }
        }

        // CRUD
        public IActionResult CardapioCrud()
        {
            return View();
        }

        public IActionResult FormEditar()
        {
            return View();
        }
    }
}