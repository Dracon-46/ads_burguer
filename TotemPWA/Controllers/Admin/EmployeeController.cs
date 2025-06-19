using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; // Necessário para SelectListItem
using TotemPWA.Data; // <-- Esta linha está correta e é essencial
using TotemPWA.Models;
using TotemPWA.ViewModels;
using System.Linq; // Necessário para LINQ .ToList(), .Select(), etc.
using System.Threading.Tasks; // Necessário para Task e async/await

namespace TotemPWA.Controllers.Admin
{
    // Lembre-se: NÃO inclua [Area("Admin")] aqui, conforme nossa configuração atual.
    [Route("Admin/[controller]/[action]")]
    public class EmployeeController : Controller
    {
        // ***** LINHA 16 - CORREÇÃO AQUI *****
        private readonly ApplicationDbContext _context; // Mude de AppDbContext para ApplicationDbContext

        // ***** LINHA 18 - CORREÇÃO AQUI *****
        public EmployeeController(ApplicationDbContext context) // Mude de AppDbContext para ApplicationDbContext
        {
            _context = context;
        }

        // GET: Admin/Employee/List
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var employees = await _context.Employees.Include(e => e.Client).ToListAsync();
            return View(employees);
        }

        // GET: Admin/Employee/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new EmployeeViewModel
            {
                Clients = await GetClientSelectListAsync()
            };
            return View(viewModel);
        }

        // POST: Admin/Employee/Create
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeViewModel viewModel)
        {
            ModelState.Remove("Employee.Client");

            if (ModelState.IsValid)
            {
                var clientExists = await _context.Clients.AnyAsync(c => c.Id == viewModel.Employee.ClientId);
                if (!clientExists)
                {
                    ModelState.AddModelError("Employee.ClientId", "Cliente selecionado não existe.");
                    viewModel.Clients = await GetClientSelectListAsync();
                    return View(viewModel);
                }

                _context.Employees.Add(viewModel.Employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List));
            }

            viewModel.Clients = await GetClientSelectListAsync();
            return View(viewModel);
        }

        // GET: Admin/Employee/Edit/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.Include(e => e.Client).FirstOrDefaultAsync(e => e.ClientId == id);
            if (employee == null) return NotFound();

            var viewModel = new EmployeeViewModel
            {
                Employee = employee,
                Clients = await GetClientSelectListAsync()
            };

            return View(viewModel);
        }

        // POST: Admin/Employee/Edit/5
        [HttpPost("{id}")]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel viewModel)
        {
            if (id != viewModel.Employee.ClientId) return BadRequest();

            ModelState.Remove("Employee.Client");

            if (ModelState.IsValid)
            {
                try
                {
                    var clientExists = await _context.Clients.AnyAsync(c => c.Id == viewModel.Employee.ClientId);
                    if (!clientExists)
                    {
                        ModelState.AddModelError("Employee.ClientId", "Cliente selecionado não existe.");
                        viewModel.Clients = await GetClientSelectListAsync();
                        return View(viewModel);
                    }
                    
                    _context.Employees.Update(viewModel.Employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(viewModel.Employee.ClientId))
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

            viewModel.Clients = await GetClientSelectListAsync();
            return View(viewModel);
        }

        // GET: Admin/Employee/Delete/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.Include(e => e.Client).FirstOrDefaultAsync(e => e.ClientId == id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Admin/Employee/ConfirmDelete/5
        [HttpPost("{id}")]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ClientId == id);
        }

        private async Task<List<SelectListItem>> GetClientSelectListAsync()
        {
            return await _context.Clients
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
    }
}