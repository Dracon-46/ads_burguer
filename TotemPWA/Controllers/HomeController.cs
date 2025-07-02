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
                _logger.LogInformation($"ValidarCupom: Cupom '{cupom.Code}' encontrado. ID: {cupom.Id}, Valor LIDO DO DB: {cupom.Value}, Tipo: {cupom.Type}.");

                decimal calculatedDesconto;
                string valorParaExibicao = ""; // Variável para o texto de exibição no frontend

                if (cupom.Type.Equals("percentual", StringComparison.OrdinalIgnoreCase))
                {
                    calculatedDesconto = cupom.Value;
                    valorParaExibicao = (cupom.Value * 100).ToString("N0") + "%";
                    _logger.LogInformation($"ValidarCupom: Cupom '{cupom.Code}' é percentual. Valor FINAL enviado para cálculo: {calculatedDesconto}. Valor para exibição: {valorParaExibicao}.");
                }
                else // Para cupons fixos (ex: R$ 10,00)
                {
                    calculatedDesconto = cupom.Value;
                    valorParaExibicao = cupom.Value.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
                    _logger.LogInformation($"ValidarCupom: Cupom '{cupom.Code}' é do tipo '{cupom.Type}'. Valor FINAL enviado para cálculo: {calculatedDesconto}. Valor para exibição: {valorParaExibicao}.");
                }

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

            Category activeSubcategory;

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
                        image = p.Image,
                        imageUrl = p.ImageUrl,
                        description = p.Description
                    })
                    .ToListAsync<object>();
            }

            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            ViewBag.CategorySlug = activeCategory.Slug;
            ViewBag.Categories = rootCategories;
            ViewBag.SubCategories = subcategories;
            ViewBag.Products = products;

            return View(cart);
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