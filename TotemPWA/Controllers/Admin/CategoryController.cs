using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TotemPWA.Models;
using TotemPWA.Data;
using TotemPWA.ViewModels;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")] // Garante o roteamento correto
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context; // ALTERADO: AppDbContext para ApplicationDbContext

        public CategoryController(ApplicationDbContext context) // ALTERADO: AppDbContext para ApplicationDbContext
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            // Inclui as subcategorias para exibir a hierarquia
            var categories = await _context.Categories
                .Include(c => c.Subcategories)
                .Include(c => c.ParentCategory) // Inclui a categoria pai também
                .Where(c => c.ParentCategoryId == null) // Apenas categorias raiz para evitar duplicação na lista principal
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CategoryViewModel
            {
                // Carrega categorias pai, excluindo a própria categoria se for um caso de edição
                Categories = _context.Categories
                    .Where(c => c.ParentCategoryId == null) // Apenas categorias raiz podem ser pais
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Adiciona validação de token anti-falsificação
        public async Task<IActionResult> Create(CategoryViewModel viewModel)
        {
            // Verifica se o slug já existe, mesmo que o ModelState seja válido
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

            // Se o modelo for inválido, recarrega as categorias para o dropdown
            viewModel.Categories = _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            return View(viewModel);
        }

        [HttpGet("{id}")] // Usa atributo de rota para pegar o ID da URL
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var viewModel = new CategoryViewModel
            {
                Category = category,
                // Carrega categorias pai, excluindo a categoria que está sendo editada
                Categories = _context.Categories
                    .Where(c => c.Id != id && c.ParentCategoryId == null) // Não pode ser pai de si mesma e apenas raiz
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost("{id}")] // Usa atributo de rota para pegar o ID da URL
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel viewModel)
        {
            // O ID deve vir do hidden field do formulário
            if (viewModel.Category.Id == 0) // Ou outro valor padrão para ID não encontrado
            {
                return BadRequest("ID da categoria não fornecido.");
            }

            // Reobtém a categoria do banco de dados para garantir que todas as propriedades sejam rastreadas
            var existingCategory = await _context.Categories.FindAsync(viewModel.Category.Id);
            if (existingCategory == null) return NotFound();

            // Evita erro de slug duplicado ao editar o nome da categoria
            // e garante que não está verificando o próprio slug do item que está sendo editado
            if (_context.Categories.Any(c => c.Slug == viewModel.Category.Slug && c.Id != viewModel.Category.Id))
            {
                ModelState.AddModelError("Category.Name", "Uma categoria com este nome já existe.");
            }

            if (ModelState.IsValid)
            {
                // Atualiza as propriedades manualmente ou usa Entry.CurrentValues.SetValues()
                existingCategory.Name = viewModel.Category.Name;
                existingCategory.Icon = viewModel.Category.Icon;
                existingCategory.ParentCategoryId = viewModel.Category.ParentCategoryId;
               // existingCategory.Slug = viewModel.Category.Slug; // O slug é gerado no setter de Name

                _context.Categories.Update(existingCategory); // Marca como modificada
                await _context.SaveChangesAsync();
                return RedirectToAction("List");
            }

            // Se o modelo for inválido, recarrega as categorias para o dropdown
            viewModel.Categories = _context.Categories
                .Where(c => c.Id != viewModel.Category.Id && c.ParentCategoryId == null)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            return View(viewModel);
        }


        [HttpGet("{id}")] // Usa atributo de rota para pegar o ID da URL
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                                .Include(c => c.ParentCategory) // Para exibir o nome da categoria pai
                                .Include(c => c.Products) // Para verificar se há produtos
                                .Include(c => c.Subcategories) // Para verificar se há subcategorias
                                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();

            // Adiciona lógica para evitar exclusão de categorias com dependências
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

        [HttpPost("{id}")] // Usa atributo de rota para pegar o ID da URL
        [ActionName("ConfirmDelete")] // Nomeia a ação para que a rota possa diferenciar
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