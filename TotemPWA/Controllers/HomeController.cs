// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels;
using TotemPWA.ViewModels;
using TotemPWA.Utilities;
using System.Linq; // Adicione este using
using System.Collections.Generic; // Adicione este using
using System; // Adicione este using para Guid
using Newtonsoft.Json;


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

        public IActionResult Cupom(decimal totalPedido, int totalItens)
        {
            ViewBag.TotalPedido = totalPedido;
            ViewBag.TotalItens = totalItens;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidarCupom([FromBody] CupomValidationRequest request)
        {
            _logger.LogInformation($"ValidarCupom: Requisição recebida para o cupom '{request.CodigoCupom}'.");

            if (string.IsNullOrWhiteSpace(request.CodigoCupom))
            {
                _logger.LogWarning("ValidarCupom: Código do cupom não pode ser vazio.");
                return Json(new { isValid = false, message = "Código do cupom não pode ser vazio." });
            }

            var cupom = await _context.Cupons
                                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CodigoCupom.ToUpper());

            if (cupom == null)
            {
                _logger.LogWarning($"ValidarCupom: Cupom com código '{request.CodigoCupom}' NÃO encontrado no banco de dados.");
                return Json(new { isValid = false, message = "Cupom não encontrado." });
            }
            else
            {
                // Log do valor exato que veio do banco de dados
                _logger.LogInformation($"ValidarCupom: Cupom '{cupom.Code}' encontrado. ID: {cupom.Id}, Valor LIDO DO DB: {cupom.Value}, Tipo: {cupom.Type}.");

                decimal calculatedDesconto;
                string valorParaExibicao = ""; // Variável para o texto de exibição no frontend

                // --- LÓGICA DE CONVERSÃO E FORMATAÇÃO PARA EXIBIÇÃO ---
                if (cupom.Type.Equals("percentual", StringComparison.OrdinalIgnoreCase))
                {
                    // Se o DB já armazena 0.5 para 50%, usamos esse valor diretamente para o cálculo.
                    calculatedDesconto = cupom.Value; 
                    // Para exibição, convertemos 0.5 para "50%"
                    valorParaExibicao = (cupom.Value * 100).ToString("N0") + "%"; 
                    _logger.LogInformation($"ValidarCupom: Cupom '{cupom.Code}' é percentual. Valor FINAL enviado para cálculo: {calculatedDesconto}. Valor para exibição: {valorParaExibicao}.");
                }
                else // Para cupons fixos (ex: R$ 10,00)
                {
                    calculatedDesconto = cupom.Value;
                    // Formata para moeda local (ex: R$ 10,00)
                    valorParaExibicao = cupom.Value.ToString("C2", new System.Globalization.CultureInfo("pt-BR")); 
                    _logger.LogInformation($"ValidarCupom: Cupom '{cupom.Code}' é do tipo '{cupom.Type}'. Valor FINAL enviado para cálculo: {calculatedDesconto}. Valor para exibição: {valorParaExibicao}.");
                }
                // --- FIM DA LÓGICA DE CONVERSÃO E FORMATAÇÃO ---

                // Retorna o 'calculatedDesconto' e 'valorParaExibicao' para o JavaScript
                return Json(new { isValid = true, message = "Cupom válido!", desconto = calculatedDesconto, tipoDesconto = cupom.Type, valorParaExibicao = valorParaExibicao });
            }
        }

        public class CupomValidationRequest
        {
            public string CodigoCupom { get; set; }
        }

        public IActionResult TelaFinal()
        {
            return View();
        }

        public IActionResult PersonalizarProdutos()
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
                        image = p.Image,       // Mantém se você usa esta propriedade também (ex: para base64)
                        imageUrl = p.ImageUrl,   // <-- ADICIONE ESTA LINHA
                        description = p.Description // <-- ADICIONE ESTA LINHA
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

        [HttpGet]
        public async Task<IActionResult> Personalizar(int productId, Guid? cartItemId) // cartItemId é opcional para edição
        {
            var product = await _context.Products
                                .Include(p => p.Additionals!) // Inclua todos os Additionals
                                    .ThenInclude(pa => pa.Ingredient) // E os Ingredientes relacionados
                                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            var viewModel = new PersonalizarProdutoViewModel
            {
                Produto = product,
                CartItemId = cartItemId ?? Guid.Empty,
                ProdutoAdditionals = product.Additionals // Passa todos os additionals do produto para a view
            };

          // Se for edição, pré-preenche o ViewModel com as personalizações atuais do carrinho
            if (cartItemId.HasValue && cartItemId.Value != Guid.Empty)
            {
                var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
                var existingCartItem = cart.FirstOrDefault(ci => ci.CartItemId == cartItemId.Value);
                if (existingCartItem != null)
                {
                    viewModel.QuantidadesManipuladas = existingCartItem.ManipulatedIngredientsWithQuantity ?? new Dictionary<int, int>();
                }
            }

            return View("PersonalizarProdutos", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarPersonalizacao(PersonalizarProdutoInputModel inputModel)
        {
            // Validação básica do modelo
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Erro nos dados de personalização. Tente novamente.";
                return RedirectToAction("Personalizar", new { productId = inputModel.ProdutoId, cartItemId = inputModel.CartItemId });
            }

            var product = await _context.Products
                                .Include(p => p.Additionals!) // Inclua todos os Additionals
                                    .ThenInclude(pa => pa.Ingredient) // E os Ingredientes relacionados
                                .FirstOrDefaultAsync(p => p.Id == inputModel.ProdutoId);

            if (product == null) return NotFound("Produto não encontrado.");

            decimal personalizedPrice = product.Price; // Começa com o preço base do produto
            var personalizationSummary = new List<string>();

            var manipulatedIngredientsWithQuantity = new Dictionary<int, int>();

            // Itera sobre os ingredientes manipulados enviados do formulário
            foreach (var entry in inputModel.IngredientesManipuladosQuantidades)
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
                        // Opcional: Adicionar um ModelState.AddModelError para informar o usuário sobre o limite
                    }

                    // Calcula o impacto no preço e adiciona ao resumo
                    // Se o ingrediente for adicionável e a quantidade for > 0, adiciona o custo
                    if (additional.CanBeAdded)
                    {
                        personalizedPrice += additional.Ingredient.Price * finalQuantity;
                        if (finalQuantity > 0)
                        {
                            personalizationSummary.Add($"{finalQuantity}x com {additional.Ingredient.Name}");
                        }
                    }
                    // Se for um ingrediente padrão e puder ser removido (e a quantidade final for 0 ou menor), registra a remoção
                    // Importante: A quantidade para um ingrediente padrão removido deve ser 0
                    else if (additional.IsDefault && additional.CanBeRemoved && finalQuantity == 0) // Alterado para finalQuantity == 0
                    {
                        personalizationSummary.Add($"sem {additional.Ingredient.Name}"); // Exemplo: "sem Queijo"
                    }
                    // Se for um ingrediente padrão e a quantidade final for > 0 (mantido), não adiciona ao summary complexo
                    // Ele já faz parte do produto base, não é uma "personalização" extra para o resumo.

                    manipulatedIngredientsWithQuantity[ingredientId] = finalQuantity; // Armazena a quantidade final manipulada
                }
            }

            var summaryText = personalizationSummary.Any() ?
            "(" + string.Join(", ", personalizationSummary) + ")" : "";

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
                    Image = product.Image ?? product.ImageUrl ?? "/images/products/default_product.png",
                    Quantity = 1,
                    CartItemId = Guid.NewGuid(),
                    ManipulatedIngredientsWithQuantity = manipulatedIngredientsWithQuantity, // SALVA O DICIONÁRIO COMPLETO
                    PersonalizationSummary = summaryText
                });
            }
            else // Editando item existente
            {
                cartItemToModify.Price = personalizedPrice;
                cartItemToModify.ManipulatedIngredientsWithQuantity = manipulatedIngredientsWithQuantity; // ATUALIZA O DICIONÁRIO COMPLETO
                cartItemToModify.PersonalizationSummary = summaryText;
            }

            HttpContext.Session.SetObject("Cart", cart);
            TempData["Message"] = "Produto personalizado e adicionado/atualizado no carrinho!";
            return RedirectToAction("Index", "Cart"); // Redireciona para o carrinho
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