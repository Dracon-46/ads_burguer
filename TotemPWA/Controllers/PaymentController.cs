using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TotemPWA.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ILogger<PaymentController> logger)
        {
            _logger = logger;
        }

        public IActionResult SelecionarPagamento()
        {
            return View();
        }

        public IActionResult TelaPagamentoCartao()
        {
            return View();
        }

        public IActionResult TelaPagamentoCartDigital()
        {
            return View();
        }

        public IActionResult TelaPagamentoPix()
        {
            return View();
        }

        public IActionResult TelaPagamentoDinheiro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ValidarPagamento(string metodo)
        {
            try
            {
                // Simple validation - in a real app, you would call a payment service here
                if (string.IsNullOrEmpty(metodo))
                {
                    TempData["Erro"] = "Método de pagamento não especificado.";
                    return RedirectToAction("SelecionarPagamento");
                }

                // Simulate random failures for demo purposes (10% chance)
                var random = new Random();
                if (random.Next(0, 10) == 0) // 10% chance of failure
                {
                    TempData["Erro"] = $"Pagamento com {metodo} não autorizado. Por favor, tente novamente ou use outro método.";
                    _logger.LogWarning($"Pagamento com {metodo} falhou (simulação)");
                    return RedirectToAction($"TelaPagamento{metodo}");
                }

                // Log successful payment
                _logger.LogInformation($"Pagamento com {metodo} realizado com sucesso");
                TempData["Sucesso"] = "Pagamento realizado com sucesso!";
                return RedirectToAction("SelecionarPagamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento");
                TempData["Erro"] = "Ocorreu um erro ao processar seu pagamento. Por favor, tente novamente.";
                return RedirectToAction("SelecionarPagamento");
            }
        }
    }
}