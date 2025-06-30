using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotemPWA.Data;
using TotemPWA.Models;

namespace TotemPWA.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")] // Garante o roteamento correto
    public class CupomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CupomController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var cupons = await _context.Cupons.ToListAsync();
            return View(cupons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cupom cupom)
        {
            // Verifica se um cupom com o mesmo código já existe
            if (_context.Cupons.Any(c => c.Code.ToUpper() == cupom.Code.ToUpper())) // Adicionado ToUpper para comparação case-insensitive
            {
                ModelState.AddModelError("Code", "Um cupom com este código já existe.");
            }

            if (!ModelState.IsValid) return View(cupom);

            // --- CORREÇÃO: Converte o valor para armazenamento no DB (se for porcentagem) ---
            // O valor recebido do formulário é o que o usuário digitou (ex: 3 para 3%)
            if (cupom.Type.Equals("percentual", StringComparison.OrdinalIgnoreCase))
            {
                cupom.Value /= 100M; // Converte 3 para 0.03
            }
            // --- FIM DA CORREÇÃO ---

            _context.Cupons.Add(cupom);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null) return NotFound();

            // --- CORREÇÃO: Converte o valor para exibição na UI (se for porcentagem) ---
            // O valor vindo do banco é 0.03, mas para exibir na UI queremos 3
            if (cupom.Type.Equals("percentual", StringComparison.OrdinalIgnoreCase))
            {
                cupom.Value *= 100M; // Converte 0.03 para 3
            }
            // --- FIM DA CORREÇÃO ---

            return View(cupom);
        }

        [HttpPost("{id}")] // Adicionado rota com ID
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cupom cupom)
        {
            if (id != cupom.Id) return BadRequest();

            // Verifica se um cupom com o mesmo código já existe, excluindo o próprio cupom
            if (_context.Cupons.Any(c => c.Code.ToUpper() == cupom.Code.ToUpper() && c.Id != cupom.Id)) // Adicionado ToUpper
            {
                ModelState.AddModelError("Code", "Um cupom com este código já existe.");
            }

            // Precisamos do estado original do cupom do banco para comparar o Type
            // Se o ModelState não for válido, não podemos confiar no 'cupom.Type' que veio do POST,
            // pois o usuário pode ter alterado o tipo no formulário, mas o valor ainda precisa ser tratado corretamente.
            var existingCupom = await _context.Cupons.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingCupom == null) // Cupom não encontrado (improvável aqui, mas boa prática)
            {
                if (!ModelState.IsValid) return View(cupom); // Retorna a view com erros de validação
                return NotFound();
            }

            // Atualiza apenas as propriedades que permitem edição para evitar sobregravar o Type se não for desejado
            // ou se o tipo for alterado e o valor não for ajustado em JS (melhor fazer no backend)
            // Certifique-se de que o Type seja o que está sendo editado.

            // --- CORREÇÃO: Converte o valor de volta para armazenamento no DB (se for porcentagem) ---
            // O valor recebido do formulário é o que o usuário digitou (ex: 3 para 3%)
            if (cupom.Type.Equals("percentual", StringComparison.OrdinalIgnoreCase))
            {
                cupom.Value /= 100M; // Converte 3 para 0.03
            }
            // --- FIM DA CORREÇÃO ---

            if (!ModelState.IsValid) // Verifica ModelState.IsValid APÓS a potencial conversão, se necessário
            {
                // Se o ModelState for inválido aqui, o valor convertido já está no 'cupom.Value'.
                // Se você retornar a view, e for um cupom percentual, o valor exibido voltaria a ser 0.03
                // por causa da lógica do GET (que não será chamada).
                // Para exibir o valor correto novamente, você precisaria reconverter:
                if (cupom.Type.Equals("percentual", StringComparison.OrdinalIgnoreCase))
                {
                    cupom.Value *= 100M; // Reconverte 0.03 para 3 para exibir novamente na view de edição
                }
                return View(cupom);
            }

            try
            {
                _context.Update(cupom);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cupons.Any(c => c.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(List));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cupom = await _context.Cupons.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id);
            if (cupom == null) return NotFound();

            // Lógica para evitar exclusão se o cupom estiver em uso
            if (cupom.Orders != null && cupom.Orders.Any())
            {
                TempData["ErrorMessage"] = "Não é possível excluir este cupom porque ele está associado a pedidos existentes.";
                return RedirectToAction(nameof(List));
            }

            return View(cupom);
        }

        [HttpPost("{id}"), ActionName("DeleteConfirmed")] // Adicionado rota com ID e ActionName
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null) return NotFound();

            _context.Cupons.Remove(cupom);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
    }
}