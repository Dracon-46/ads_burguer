using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;
using TotemPWA.ViewModels; // <-- Importante: Adicionar este using
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Para List<SelectListItem>
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem

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

        // GET: Admin/Category/List
        public async Task<IActionResult> List()
        {
            var categories = await _context.Categories
                                           .Include(c => c.Subcategories)
                                           .Include(c => c.ParentCategory)
                                           .ToListAsync();
            
            ViewBag.MainCategories = await _context.Categories
                                                   .Where(c => c.ParentCategoryId == null)
                                                   .OrderBy(c => c.Name)
                                                   .ToListAsync();
            
            return View(categories);
        }

        // GET: Admin/Category/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CategoryViewModel();
            // Carrega as categorias principais para o dropdown
            viewModel.ParentCategoryOptions = await _context.Categories
                                                           .Where(c => c.ParentCategoryId == null)
                                                           .OrderBy(c => c.Name)
                                                           .Select(c => new SelectListItem
                                                           {
                                                               Value = c.Id.ToString(),
                                                               Text = c.Name
                                                           })
                                                           .ToListAsync();
            
            // Adiciona a opção "Nenhuma (Categoria Principal)" para o dropdown
            viewModel.ParentCategoryOptions.Insert(0, new SelectListItem { Value = "0", Text = "Nenhuma (Categoria Principal)" });
            
            return View(viewModel); // Passa o ViewModel para a view
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Agora recebe CategoryViewModel do formulário
        public async Task<IActionResult> Create(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var category = new Category // Cria uma nova instância de Category (modelo de domínio)
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description
                    // O Slug será gerado automaticamente pelo setter de Name na classe Category
                };

                // Trata o ParentCategoryId do ViewModel
                if (viewModel.ParentCategoryId.HasValue && viewModel.ParentCategoryId.Value != 0)
                {
                    category.ParentCategoryId = viewModel.ParentCategoryId;
                }
                else
                {
                    category.ParentCategoryId = null; // Garante que seja null se "0" for selecionado
                }
                
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Categoria criada com sucesso!";
                return RedirectToAction(nameof(List));
            }

            // Se o ModelState não for válido, recarrega as opções do dropdown para retornar à view
            viewModel.ParentCategoryOptions = await _context.Categories
                                                           .Where(c => c.ParentCategoryId == null)
                                                           .OrderBy(c => c.Name)
                                                           .Select(c => new SelectListItem
                                                           {
                                                               Value = c.Id.ToString(),
                                                               Text = c.Name
                                                           })
                                                           .ToListAsync();
            viewModel.ParentCategoryOptions.Insert(0, new SelectListItem { Value = "0", Text = "Nenhuma (Categoria Principal)" });

            return View(viewModel); // Retorna o ViewModel com erros de validação
        }

        // GET: Admin/Category/Edit/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Mapeia o modelo de domínio (Category) para o ViewModel
            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId
            };

            // Popula as opções do dropdown para edição
            // Exclui a própria categoria da lista de pais para evitar recursão
            viewModel.ParentCategoryOptions = await _context.Categories
                                                           .Where(c => c.ParentCategoryId == null && c.Id != category.Id)
                                                           .OrderBy(c => c.Name)
                                                           .Select(c => new SelectListItem
                                                           {
                                                               Value = c.Id.ToString(),
                                                               Text = c.Name
                                                           })
                                                           .ToListAsync();
            viewModel.ParentCategoryOptions.Insert(0, new SelectListItem { Value = "0", Text = "Nenhuma (Categoria Principal)" });

            return View(viewModel); // Passa o ViewModel para a view
        }

        // POST: Admin/Category/Edit/5
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        // Agora recebe CategoryViewModel do formulário
        public async Task<IActionResult> Edit(int id, CategoryViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Carrega a Category existente do banco de dados para atualizar
                    var categoryToUpdate = await _context.Categories.FindAsync(viewModel.Id);
                    if (categoryToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Atualiza as propriedades do modelo de domínio com os dados do ViewModel
                    categoryToUpdate.Name = viewModel.Name; // Isso vai gerar o novo Slug
                    categoryToUpdate.Description = viewModel.Description;

                    // Trata o ParentCategoryId do ViewModel
                    if (viewModel.ParentCategoryId.HasValue && viewModel.ParentCategoryId.Value != 0)
                    {
                        categoryToUpdate.ParentCategoryId = viewModel.ParentCategoryId;
                    }
                    else
                    {
                        categoryToUpdate.ParentCategoryId = null; // Garante que seja null
                    }
                    
                    _context.Update(categoryToUpdate); // O EF rastreia as mudanças e as salvará
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Categoria atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(List));
            }
            // Se o ModelState não for válido, recarrega as opções do dropdown
            viewModel.ParentCategoryOptions = await _context.Categories
                                                           .Where(c => c.ParentCategoryId == null && c.Id != viewModel.Id)
                                                           .OrderBy(c => c.Name)
                                                           .Select(c => new SelectListItem
                                                           {
                                                               Value = c.Id.ToString(),
                                                               Text = c.Name
                                                           })
                                                           .ToListAsync();
            viewModel.ParentCategoryOptions.Insert(0, new SelectListItem { Value = "0", Text = "Nenhuma (Categoria Principal)" });

            return View(viewModel); // Retorna o ViewModel com erros de validação
        }
        
        // --- MÉTODOS DE EXCLUSÃO ---

        // GET: Admin/Category/Delete/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory) 
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (category == null)
            {
                return NotFound();
            }

            return View(category); // Retorna a view Delete.cshtml para confirmação
        }

        // POST: Admin/Category/Delete/5
        [HttpPost("{id}")]
        [ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Antes de excluir a categoria, lida com suas subcategorias, tornando-as principais.
                var subcategories = await _context.Categories
                                                     .Where(c => c.ParentCategoryId == id)
                                                     .ToListAsync();
                foreach (var subcat in subcategories)
                {
                    subcat.ParentCategoryId = null; 
                }
                _context.Categories.UpdateRange(subcategories);
                await _context.SaveChangesAsync(); 

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Categoria excluída com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Categoria não encontrada para exclusão.";
            }
            return RedirectToAction(nameof(List));
        }

        // Método auxiliar para verificar se uma categoria existe.
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}