using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 
using TotemPWA.Data; 
using TotemPWA.Models;
using TotemPWA.ViewModels;
using System.Linq; 
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Cookies; 
using System.Security.Claims; 

namespace TotemPWA.Controllers.Admin
{
    // Lembre-se: NÃO inclua [Area("Admin")] aqui, conforme nossa configuração atual.
    [Authorize]  // Protege o controlador, requerendo autenticação para acessar as ações
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
            ModelState.Remove("Employee.Client"); // Remove a validação do objeto de navegação

            if (ModelState.IsValid) // Verifica a validade do modelo
            {
                var clientExists = await _context.Clients.AnyAsync(c => c.Id == viewModel.Employee.ClientId); // Verifica se o cliente existe
                if (!clientExists)
                {
                    ModelState.AddModelError("Employee.ClientId", "Cliente selecionado não existe."); // Adiciona erro se o cliente não existe
                    viewModel.Clients = await GetClientSelectListAsync(); // Recarrega a lista de clientes
                    return View(viewModel); // Retorna a view com o erro
                }

                _context.Employees.Add(viewModel.Employee); // Adiciona o funcionário
                await _context.SaveChangesAsync(); // Salva as mudanças
                return RedirectToAction(nameof(List)); // Redireciona para a lista
            }

            viewModel.Clients = await GetClientSelectListAsync(); // Recarrega a lista de clientes se o modelo for inválido
            return View(viewModel); // Retorna a view com erros de validação
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

        // --- AÇÕES DE LOGIN E LOGOUT MOVIDAS PARA DENTRO DA CLASSE ---

        // GET: Admin/Employee/Login
        [HttpGet]
        [AllowAnonymous] // Permite acesso sem autenticação
        public IActionResult Login()
        {
            // Se o usuário já estiver autenticado, redireciona para a área de funcionários
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin"); 
            }
            // O código assume a existência de TotemPWA.ViewModels.LoginViewModel
            return View(new LoginViewModel());
        }

        // POST: Admin/Employee/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Buscar o funcionário pelo nome de usuário
            // Usamos .Include(e => e.Client) para garantir que temos acesso aos dados do cliente.
            var employee = await _context.Employees
                .Include(e => e.Client)
                .FirstOrDefaultAsync(e => e.User == model.User);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                return View(model);
            }

            // 2. Verificar a senha (a senha é comparada diretamente com o Employee.Password)
            if (employee.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                return View(model);
            }

            // 3. Autenticar o usuário
            // Criar as claims (identidade) do usuário
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, employee.User),
                new Claim(ClaimTypes.NameIdentifier, employee.ClientId.ToString()),
                // Adicionar o cargo do funcionário como ClaimTypes.Role para autorização
                new Claim(ClaimTypes.Role, employee.Type) 
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties {};

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirecionar para a área protegida após o login
            return RedirectToAction("Index", "Admin"); 
        }

        // POST: Admin/Employee/Logout
        [HttpPost]
        // Remove a autenticação
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Redireciona para a tela de login
            return RedirectToAction("Login", "Employee");
        }
    }
}