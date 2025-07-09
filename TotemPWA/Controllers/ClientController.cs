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
            var clients = await _context.Clients.ToListAsync();
            return View(clients);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();

            if (!ModelState.IsValid) return View(client);

            _context.Update(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id);
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
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        // ClientController.cs - Adicione este método
        [HttpPost]
        public async Task<IActionResult> CheckCpfExistence([FromBody] CpfCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cpf))
            {
                return Json(new { exists = false, message = "CPF não fornecido." });
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.CPF == request.Cpf);
            if (client != null)
            {
                return Json(new { exists = true, clientName = client.Name });
            }
            return Json(new { exists = false });
        }

        // ClientController.cs - Novo Endpoint para Registro de Novo Cliente
        [HttpPost]
        public async Task<IActionResult> RegisterNewClient([FromBody] ClientRegistrationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Cpf))
            {
                return Json(new { success = false, message = "Nome e CPF são obrigatórios." });
            }

            // Validação básica do CPF no servidor (opcional, mas boa prática)
            // Você pode integrar a mesma validação de algoritmo de CPF aqui, se necessário.

            // Verifica se o CPF já existe para evitar duplicatas
            if (await _context.Clients.AnyAsync(c => c.CPF == request.Cpf))
            {
                return Json(new { success = false, message = "CPF já cadastrado." });
            }

            var newClient = new Client
            {
                Name = request.Name,
                CPF = request.Cpf
            };

            _context.Clients.Add(newClient);
            await _context.SaveChangesAsync();

            // Se este for um fluxo de registro de funcionário, você criaria um registro de Employee
            // Isso seria normalmente tratado em uma etapa separada ou através de um endpoint diferente
            // que lida especificamente com a criação de funcionários.
            // Para um registro geral de "novo cliente", isso é suficiente.

            return Json(new { success = true, clientId = newClient.Id, clientName = newClient.Name });
        }

        // ClientController.cs - Adicione esses DTOs no final do arquivo ou em uma pasta separada
        public class CpfCheckRequest
        {
            public string Cpf { get; set; }
        }

        public class ClientRegistrationRequest
        {
            public string Name { get; set; }
            public string Cpf { get; set; }
        }
    }
}
