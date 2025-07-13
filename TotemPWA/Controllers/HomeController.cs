// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.Models.ViewModels;
using TotemPWA.ViewModels;
using TotemPWA.Utilities;
using System.Linq;
using System.Collections.Generic;
using System;
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
        public IActionResult TelaCPFNaNota()
        {
            return View();
        }

        public IActionResult TelaCPF()
        {
            // Opcionalmente, pode passar o CPF salvo na sessão para preenchimento prévio
            var cpfData = HttpContext.Session.GetObject<CPFSessionData>("CPFData");
            return View(cpfData);
        }

        // Ação para processar o formulário de CPF e salvar na sessão
        [HttpPost]
        public IActionResult ProcessarCPF(string cpf, string nome)
        {
            if (string.IsNullOrWhiteSpace(cpf) || string.IsNullOrWhiteSpace(nome))
            {
                // Tratar erro se os campos estiverem vazios
                TempData["ErroCPF"] = "Nome e CPF são obrigatórios.";
                return RedirectToAction("TelaCPF");
            }

            var cpfData = new CPFSessionData
            {
                Nome = nome,
                CPF = cpf
            };

            // Salva os dados do CPF na sessão
            HttpContext.Session.SetObject("CPFData", cpfData);
            
            // Redireciona para a próxima tela (por exemplo, SelecionarPedido ou TelaProduto)
            // Ajuste o redirecionamento conforme seu fluxo de aplicação
            return RedirectToAction("SelecionarPedido"); 
        }

        [HttpPost]
        public IActionResult SalvarCPFNota(string cpf, string nome)
        {
            var cpfData = new CPFSessionData
            {
                Nome = nome,
                CPF = cpf
            };

            // Salva os dados do CPF na sessão C#
            // Importante: Isso sobrescreve qualquer CPFData anterior se o usuário mudar de ideia.
            HttpContext.Session.SetObject("CPFData", cpfData);
            
            // Redireciona para o carrinho/seleção de pedido
            return RedirectToAction("SelecionarPedido");
        }

         [HttpPost]
        public IActionResult SalvarDadosCliente([FromBody] CPFSessionData dadosCliente)
        {
            if (dadosCliente == null || string.IsNullOrWhiteSpace(dadosCliente.Nome))
            {
                return BadRequest(new { success = false, message = "Dados do cliente inválidos." });
            }

            // Salva os dados na sessão C#
            HttpContext.Session.SetObject("CPFData", dadosCliente);

            return Json(new { success = true });
        }

        public IActionResult TelaNome()
        {
            return View();
        }
        public IActionResult TelaNomeClube()
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

        [HttpGet]
        public IActionResult ResetSessionAndRedirect()
        {
            // Limpa toda a sessão
            HttpContext.Session.Clear();
            
            // Redireciona para a página inicial
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Cupom()
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var cupomData = HttpContext.Session.GetObject<CupomSessionData>("CupomData");
            
            var viewModel = new CupomViewModel
            {
                Cart = cart,
                CupomData = cupomData,
                TotalItens = cart.Sum(x => x.Quantity),
                TotalPedido = cart.Sum(x => x.Price * x.Quantity)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ValidarCupom(string codigoCupom)
        {
            _logger.LogInformation($"ValidarCupom: Requisição recebida para o cupom '{codigoCupom}'.");

            if (string.IsNullOrWhiteSpace(codigoCupom))
            {
                _logger.LogWarning("ValidarCupom: Código do cupom não pode ser vazio.");
                TempData["CupomErro"] = "Código do cupom não pode ser vazio.";
                return RedirectToAction("Cupom");
            }

            var cupom = await _context.Cupons
                                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == codigoCupom.ToUpper());

            if (cupom == null)
            {
                _logger.LogWarning($"ValidarCupom: Cupom com código '{codigoCupom}' NÃO encontrado no banco de dados.");
                TempData["CupomErro"] = "Cupom não encontrado.";
                return RedirectToAction("Cupom");
            }

            // Calcular desconto
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var subtotal = cart.Sum(x => x.Price * x.Quantity);
            
            decimal valorDesconto = 0;
            string valorParaExibicao = "";

            if (cupom.Type.Equals("percentual", StringComparison.OrdinalIgnoreCase))
            {
                valorDesconto = subtotal * cupom.Value;
                valorParaExibicao = (cupom.Value * 100).ToString("N0") + "%";
            }
            else
            {
                valorDesconto = cupom.Value;
                valorParaExibicao = cupom.Value.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
            }

            // Garantir que o desconto não seja maior que o total
            valorDesconto = Math.Min(valorDesconto, subtotal);
            var totalComDesconto = subtotal - valorDesconto;

            // Salvar dados do cupom na sessão
            var cupomData = new CupomSessionData
            {
                Codigo = cupom.Code,
                Desconto = valorDesconto,
                TipoDesconto = cupom.Type,
                ValorParaExibicao = valorParaExibicao,
                Subtotal = subtotal,
                TotalComDesconto = totalComDesconto,
                IsValid = true
            };

            HttpContext.Session.SetObject("CupomData", cupomData);
            
            _logger.LogInformation($"ValidarCupom: Cupom '{cupom.Code}' aplicado com sucesso. Desconto: {valorDesconto:C2}");
            TempData["CupomSucesso"] = $"Cupom aplicado com sucesso! Desconto de {valorParaExibicao}.";

            return RedirectToAction("Cupom");
        }

        [HttpPost]
        public IActionResult RemoverCupom()
        {
            HttpContext.Session.Remove("CupomData");
            TempData["CupomInfo"] = "Cupom removido com sucesso.";
            return RedirectToAction("Cupom");
        }

        // Método para obter dados do pedido (usado em outras views)
        public IActionResult GetPedidoInfo()
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var cupomData = HttpContext.Session.GetObject<CupomSessionData>("CupomData");
            
            var totalItens = cart.Sum(x => x.Quantity);
            var subtotal = cart.Sum(x => x.Price * x.Quantity);
            var totalFinal = cupomData?.TotalComDesconto ?? subtotal;

            return Json(new
            {
                totalItens = totalItens,
                subtotal = subtotal,
                totalFinal = totalFinal,
                temCupom = cupomData?.IsValid == true,
                cupomDesconto = cupomData?.Desconto ?? 0,
                cupomValor = cupomData?.ValorParaExibicao ?? ""
            });
        }

        public IActionResult TelaFinal()
        {
            return View();
        }

        [HttpGet("TelaProduto/{categorySlug?}/{subcategorySlug?}")]
        public async Task<IActionResult> TelaProduto(string categorySlug, string subcategorySlug = null)
        {
            // 1. Encontrar a categoria ativa usando o SLUG
            var rootCategoriesRaw = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();

            Category? activeCategory = null;
            if (!string.IsNullOrEmpty(categorySlug))
            {
                activeCategory = rootCategoriesRaw.FirstOrDefault(c => c.Slug == categorySlug);
            }
            else
            {
                activeCategory = rootCategoriesRaw.FirstOrDefault();
            }
            
            if (activeCategory == null) return NotFound("Categoria não encontrada.");
            
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

            Category? activeSubcategory = null;

            if (!string.IsNullOrEmpty(subcategorySlug))
            {
                activeSubcategory = subcategoriesRaw.FirstOrDefault(s => s.Slug == subcategorySlug);
            }
            else
            {
                activeSubcategory = subcategoriesRaw.FirstOrDefault();
            }

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
            var products = new List<ProductDisplayViewModel>();
            if (activeSubcategoryId != null)
            {
                var isComboCategory = activeSubcategory?.Name?.ToLower().Contains("combos") == true;

                if (isComboCategory)
                {
                    var combosData = await _context.Combos
                        .Include(c => c.ProductCombo)
                            .ThenInclude(pc => pc.Promotions)
                        .Include(c => c.Product)
                        .Where(c => c.ProductCombo != null && c.ProductCombo.Active)
                        .GroupBy(c => c.ProductComboId)
                        .Select(g => new
                        {
                            ProductComboId = g.Key,
                            ComboProduct = g.First().ProductCombo,
                            IncludedProducts = g.Select(c => new
                            {
                                Id = c.Product!.Id,
                                Name = c.Product.Name,
                                Price = c.Product.Price,
                                ImageUrl = c.Product.ImageUrl
                            }).ToList()
                        })
                        .ToListAsync();

                        products = combosData.Select(combo => {
                            var originalPrice = combo.ComboProduct?.Price ?? 0;
                            var activePromotion = combo.ComboProduct?.Promotions?
                                .FirstOrDefault(p => p.ValidUntil >= DateTime.Today);
                            
                            var finalPrice = originalPrice;
                            if (activePromotion != null)
                            {
                                finalPrice = originalPrice - (originalPrice * activePromotion.Percent / 100);
                            }

                            return new ProductDisplayViewModel
                            {
                                Id = combo.ProductComboId,
                                Name = combo.ComboProduct?.Name ?? "Combo",
                                Price = finalPrice,
                                OriginalPrice = activePromotion != null ? originalPrice : (decimal?)null,
                                ImageUrl = combo.ComboProduct?.ImageUrl,
                                Description = $"Combo contendo: {string.Join(", ", combo.IncludedProducts.Select(p => p.Name))}",
                                IsCombo = true,
                                HasPromotion = activePromotion != null,
                                PromotionPercent = activePromotion?.Percent,
                                ComboItems = combo.IncludedProducts.Select(p => new IncludedProductViewModel
                                {
                                    ProductId = p.Id,
                                    ProductName = p.Name,
                                    ProductPrice = p.Price
                                }).ToList()
                            };
                        }).ToList();
                }
                else
                {
                    var productsWithPromotions = await _context.Products
                        .Include(p => p.Promotions)
                        .Where(p => p.CategoryId == activeSubcategoryId && p.Active)
                        .ToListAsync();

                    products = productsWithPromotions.Select(p => {
                        var activePromotion = p.Promotions?
                            .FirstOrDefault(pr => pr.ValidUntil >= DateTime.Today);
                        
                        var finalPrice = p.Price;
                        if (activePromotion != null)
                        {
                            finalPrice = p.Price - (p.Price * activePromotion.Percent / 100);
                        }

                        return new ProductDisplayViewModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Price = finalPrice,
                            OriginalPrice = activePromotion != null ? p.Price : (decimal?)null,
                            ImageUrl = p.ImageUrl,
                            Description = p.Description,
                            IsCombo = false,
                            HasPromotion = activePromotion != null,
                            PromotionPercent = activePromotion?.Percent
                        };
                    }).ToList();
                }
            }

            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            ViewBag.CategorySlug = activeCategory.Slug;
            ViewBag.Categories = rootCategories;
            ViewBag.SubCategories = subcategories;
            ViewBag.Products = products;

            return View(cart);
        }
    
        public IActionResult CardapioCrud()
        {
            return View();
        }

        public IActionResult FormEditar()
        {
            return View();
        }
    }

    // Classe para dados do cupom na sessão
    public class CupomSessionData
    {
        public string Codigo { get; set; } = string.Empty;
        public decimal Desconto { get; set; }
        public string TipoDesconto { get; set; } = string.Empty;
        public string ValorParaExibicao { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal TotalComDesconto { get; set; }
        public bool IsValid { get; set; }
    }

    // ViewModel para a view de cupom
    public class CupomViewModel
    {
        public List<CartItemViewModel> Cart { get; set; } = new List<CartItemViewModel>();
        public CupomSessionData? CupomData { get; set; }
        public int TotalItens { get; set; }
        public decimal TotalPedido { get; set; }
    }
}