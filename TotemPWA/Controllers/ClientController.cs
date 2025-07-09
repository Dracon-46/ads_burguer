using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var clients = await _context.Clients
                .Include(c => c.Employee)
                .Include(c => c.Orders)
                .ToListAsync();
            return View(clients);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            // Verificar se CPF já existe
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.CPF == client.CPF);

            if (existingClient != null)
            {
                ModelState.AddModelError("CPF", "CPF já cadastrado no sistema.");
                return View(client);
            }

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();

            if (!ModelState.IsValid) return View(client);

            // Verificar se CPF já existe em outro cliente
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.CPF == client.CPF && c.Id != id);

            if (existingClient != null)
            {
                ModelState.AddModelError("CPF", "CPF já cadastrado em outro cliente.");
                return View(client);
            }

            _context.Update(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Orders)
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (client == null) return NotFound();

            if (client.Orders != null && client.Orders.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este cliente pois ele possui pedidos associados.";
                return RedirectToAction(nameof(List));
            }

            return View(client);
        }

        [HttpPost("{id}"), ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (client == null) return NotFound();

            // Se for funcionário, remover primeiro da tabela Employee
            if (client.Employee != null)
            {
                _context.Employees.Remove(client.Employee);
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        /// <summary>
        /// Verifica se o CPF existe no sistema e retorna informações completas
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckCpfExistence([FromBody] CpfCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cpf))
            {
                return Json(new { exists = false, message = "CPF não fornecido." });
            }

            var client = await _context.Clients
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.CPF == request.Cpf);

            if (client != null)
            {
                return Json(new { 
                    exists = true, 
                    clientId = client.Id,
                    clientName = client.Name,
                    isEmployee = client.Employee != null,
                    employeeType = client.Employee?.Type
                });
            }

            return Json(new { exists = false });
        }

        /// <summary>
        /// Registra um novo cliente e, opcionalmente, cria um funcionário
        /// Implementa o conceito de especialização parcial
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegisterNewClient([FromBody] ClientRegistrationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Cpf))
            {
                return Json(new { success = false, message = "Nome e CPF são obrigatórios." });
            }

            // Verificar se o CPF já existe
            if (await _context.Clients.AnyAsync(c => c.CPF == request.Cpf))
            {
                return Json(new { success = false, message = "CPF já cadastrado no sistema." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 1. Criar o cliente (sempre necessário)
                var newClient = new Client
                {
                    Name = request.Name,
                    CPF = request.Cpf
                };

                _context.Clients.Add(newClient);
                await _context.SaveChangesAsync();

                // 2. Se for funcionário, criar o registro de Employee (especialização)
                if (request.IsEmployee && !string.IsNullOrWhiteSpace(request.EmployeeType))
                {
                    var employee = new Employee
                    {
                        ClientId = newClient.Id,
                        Type = request.EmployeeType,
                        User = GenerateUserFromName(request.Name),
                        Password = GenerateInitialPassword() // Gerar senha inicial
                    };

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    clientId = newClient.Id, 
                    clientName = newClient.Name,
                    isEmployee = request.IsEmployee,
                    employeeType = request.EmployeeType
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Erro ao registrar cliente: {ex.Message}");
                return Json(new { success = false, message = "Erro interno ao cadastrar cliente." });
            }
        }

        /// <summary>
        /// Promove um cliente existente para funcionário
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PromoteToEmployee([FromBody] PromoteToEmployeeRequest request)
        {
            if (request.ClientId <= 0 || string.IsNullOrWhiteSpace(request.EmployeeType))
            {
                return Json(new { success = false, message = "ClientId e EmployeeType são obrigatórios." });
            }

            var client = await _context.Clients
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == request.ClientId);

            if (client == null)
            {
                return Json(new { success = false, message = "Cliente não encontrado." });
            }

            if (client.Employee != null)
            {
                return Json(new { success = false, message = "Cliente já é funcionário." });
            }

            try
            {
                var employee = new Employee
                {
                    ClientId = client.Id,
                    Type = request.EmployeeType,
                    User = GenerateUserFromName(client.Name),
                    Password = GenerateInitialPassword()
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Cliente promovido a funcionário com sucesso.",
                    employeeType = request.EmployeeType
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao promover cliente: {ex.Message}");
                return Json(new { success = false, message = "Erro interno ao promover cliente." });
            }
        }

        /// <summary>
        /// Remove um funcionário (mas mantém como cliente)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DemoteFromEmployee([FromBody] DemoteFromEmployeeRequest request)
        {
            if (request.ClientId <= 0)
            {
                return Json(new { success = false, message = "ClientId é obrigatório." });
            }

            var client = await _context.Clients
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == request.ClientId);

            if (client == null)
            {
                return Json(new { success = false, message = "Cliente não encontrado." });
            }

            if (client.Employee == null)
            {
                return Json(new { success = false, message = "Cliente não é funcionário." });
            }

            try
            {
                _context.Employees.Remove(client.Employee);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Funcionário removido com sucesso. Cliente mantido no sistema."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao remover funcionário: {ex.Message}");
                return Json(new { success = false, message = "Erro interno ao remover funcionário." });
            }
        }

        /// <summary>
        /// Gera um nome de usuário baseado no nome do cliente
        /// </summary>
        private string GenerateUserFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "user";
            
            var parts = name.Trim().Split(' ');
            if (parts.Length >= 2)
            {
                return $"{parts[0].ToLower()}.{parts[parts.Length - 1].ToLower()}";
            }
            return parts[0].ToLower();
        }

        /// <summary>
        /// Gera uma senha inicial para o funcionário
        /// </summary>
        private string GenerateInitialPassword()
        {
            return "123456"; // Em produção, usar um gerador de senha mais seguro
        }

        // DTOs para as requisições
        public class CpfCheckRequest
        {
            public string Cpf { get; set; } = string.Empty;
        }

        public class ClientRegistrationRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Cpf { get; set; } = string.Empty;
            public bool IsEmployee { get; set; }
            public string? EmployeeType { get; set; }
        }

        public class PromoteToEmployeeRequest
        {
            public int ClientId { get; set; }
            public string EmployeeType { get; set; } = string.Empty;
        }

        public class DemoteFromEmployeeRequest
        {
            public int ClientId { get; set; }
        }
    }
}