using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;

namespace TotemPWA.Controllers;

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
    //  public IActionResult TelaProduto()
    // {
    //     return View();
    // }

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

    public IActionResult PersonalizarCombo()
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


    [HttpGet("TelaProduto/{categorySlug?}/{subcategorySlug?}")]     // <<< USAR ESTA NOVA ROTA
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
                slug = c.Slug, // <<< Importante passar o slug para a View
                icon = c.Icon,
                active = c.Id == activeCategoryId // A lógica de 'active' continua a mesma
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
                slug = c.Slug, // <<< Importante passar o slug para a View
                icon = c.Icon,
                active = c.Id == activeSubcategoryId
            })
            .ToList();

        // 3. Buscar produtos com base no ID da subcategoria ativa
        // (A lógica aqui não muda, pois já depende do ID que acabamos de descobrir)
        var products = new List<object>(); // Inicia uma lista vazia
        if (activeSubcategoryId != null)
        {
            products = await _context.Products
                .Where(p => p.CategoryId == activeSubcategoryId)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price
                })
                .ToListAsync<object>(); // Converte para lista de objetos
        }

        // 4. Passar os dados para a ViewBag (usando o slug da categoria principal)
        ViewBag.CategorySlug = activeCategory.Slug; // <<< MUDANÇA: Envie o SLUG, não o ID.
        ViewBag.Categories = rootCategories;
        ViewBag.SubCategories = subcategories;
        ViewBag.Products = products;

        // return Ok(
        //     new
        //     {
        //         CategorySlug = activeCategory.Slug,
        //         Categories = rootCategories,
        //         SubCategories = subcategories,
        //         Products = products
        //     }
        // );

        return View();
    }

    // crud 

    public IActionResult CardapioCrud()
    {
        return View();
    }

    public IActionResult FormEditar()
    {
        return View();
    }
}