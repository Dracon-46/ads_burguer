using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TotemPWA.Models;
using TotemPWA.Data;
using TotemPWA.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var categories = await _context.Categories
                .Include(c => c.Subcategories)
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.MainCategories = categories.Where(c => c.ParentCategoryId == null).OrderBy(c => c.Name).ToList();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CategoryViewModel
            {
                Categories = _context.Categories
                    .Where(c => c.ParentCategoryId == null)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel viewModel)
        {
            if (_context.Categories.Any(c => c.Slug == viewModel.Category.Slug))
            {
                ModelState.AddModelError("Category.Name", "Uma categoria com este nome já existe.");
            }

            if (ModelState.IsValid)
            {
                _context.Categories.Add(viewModel.Category);
                await _context.SaveChangesAsync();
                return RedirectToAction("List");
            }

            viewModel.Categories = _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var viewModel = new CategoryViewModel
            {
                Category = category,
                Categories = _context.Categories
                    .Where(c => c.Id != id && c.ParentCategoryId == null)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel viewModel)
        {
            if (viewModel.Category.Id == 0)
            {
                return BadRequest("ID da categoria não fornecido.");
            }

            var existingCategory = await _context.Categories.FindAsync(viewModel.Category.Id);
            if (existingCategory == null) return NotFound();

            if (_context.Categories.Any(c => c.Slug == viewModel.Category.Slug && c.Id != viewModel.Category.Id))
            {
                ModelState.AddModelError("Category.Name", "Uma categoria com este nome já existe.");
            }

            if (ModelState.IsValid)
            {
                existingCategory.Name = viewModel.Category.Name;
                // Linha para Icon removida.
                existingCategory.ParentCategoryId = viewModel.Category.ParentCategoryId;

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction("List");
            }

            viewModel.Categories = _context.Categories
                .Where(c => c.Id != viewModel.Category.Id && c.ParentCategoryId == null)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            return View(viewModel);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                                .Include(c => c.ParentCategory)
                                .Include(c => c.Products)
                                .Include(c => c.Subcategories)
                                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();

            if (category.Products != null && category.Products.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir esta categoria porque ela contém produtos.";
                return RedirectToAction("List");
            }
            if (category.Subcategories != null && category.Subcategories.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir esta categoria porque ela contém subcategorias.";
                return RedirectToAction("List");
            }

            return View(category);
        }

        [HttpPost("{id}")]
        [ActionName("ConfirmDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("List");
        }
    }
}