using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TotemPWA.Models.ViewModels;
using TotemPWA.ViewModels;
using TotemPWA.Utilities;
using TotemPWA.Controllers;

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
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var cupomData = HttpContext.Session.GetObject<CupomSessionData>("CupomData");
            
            if (cart.Count == 0)
            {
                TempData["Erro"] = "Seu carrinho está vazio. Adicione itens antes de continuar.";
                return RedirectToAction("TelaProduto", "Home");
            }

            var viewModel = new PagamentoViewModel
            {
                Cart = cart,
                CupomData = cupomData,
                TotalItens = cart.Sum(x => x.Quantity),
                Subtotal = cart.Sum(x => x.Price * x.Quantity),
                TotalFinal = cupomData?.TotalComDesconto ?? cart.Sum(x => x.Price * x.Quantity)
            };

            return View(viewModel);
        }

        public IActionResult TelaPagamentoCartao()
        {
            var dados = GetDadosPagamento();
            return View(dados);
        }

        public IActionResult TelaPagamentoCartDigital()
        {
            var dados = GetDadosPagamento();
            return View(dados);
        }

        public IActionResult TelaPagamentoPix()
        {
            var dados = GetDadosPagamento();
            return View(dados);
        }

        public IActionResult TelaPagamentoDinheiro()
        {
            var dados = GetDadosPagamento();
            return View(dados);
        }

        public IActionResult TelaNotaFiscal()
        {
            var pagamentoData = HttpContext.Session.GetObject<PagamentoSessionData>("PagamentoData");
            var cpfData = HttpContext.Session.GetObject<CPFSessionData>("CPFData");
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            if (pagamentoData == null)
            {
                TempData["Erro"] = "Nenhum dado de pagamento encontrado.";
                return RedirectToAction("SelecionarPagamento");
            }

            var notaFiscalViewModel = new NotaFiscalViewModel
            {
                NomeComprador = cpfData?.Nome ?? "Não informado",
                CPFComprador = cpfData?.CPF ?? "Não informado",
                Subtotal = pagamentoData.Subtotal,
                TotalFinal = pagamentoData.TotalFinal,
                DescontoAplicado = pagamentoData.CupomAplicado?.Desconto ?? 0,
                MetodoPagamento = pagamentoData.Metodo,
                NumeroTransacao = pagamentoData.NumeroTransacao,
                DataPagamento = pagamentoData.DataPagamento,
                Produtos = cart
            };

            // Opcional: Limpar a sessão após a finalização (comentado se você quiser manter os dados temporariamente)
            HttpContext.Session.Clear(); 

            return View(notaFiscalViewModel);
        }

        [HttpPost]
        public IActionResult ValidarPagamento(string metodo)
        {
            try
            {
                var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
                var cupomData = HttpContext.Session.GetObject<CupomSessionData>("CupomData");

                if (cart.Count == 0)
                {
                    TempData["Erro"] = "Seu carrinho está vazio.";
                    return RedirectToAction("SelecionarPagamento");
                }

                if (string.IsNullOrEmpty(metodo))
                {
                    TempData["Erro"] = "Método de pagamento não especificado.";
                    return RedirectToAction("SelecionarPagamento");
                }

                var subtotal = cart.Sum(x => x.Price * x.Quantity);
                var totalFinal = cupomData?.TotalComDesconto ?? subtotal;

                if (totalFinal <= 0)
                {
                    TempData["Erro"] = "O valor total do pedido não pode ser zero.";
                    return RedirectToAction("SelecionarPagamento");
                }

                // Logar as informações do pagamento
                _logger.LogInformation($"ValidarPagamento: Método: {metodo}, Subtotal: {subtotal:C2}, Total Final: {totalFinal:C2}");
                
                if (cupomData?.IsValid == true)
                {
                    _logger.LogInformation($"ValidarPagamento: Cupom aplicado - Código: {cupomData.Codigo}, Desconto: {cupomData.Desconto:C2}, Tipo: {cupomData.TipoDesconto}");
                }

                // Simulate random failures for demo purposes (10% chance)
                var random = new Random();
                if (random.Next(0, 10) == 0) // 10% chance of failure
                {
                    TempData["Erro"] = $"Pagamento com {metodo} não autorizado. Por favor, tente novamente ou use outro método.";
                    _logger.LogWarning($"Pagamento com {metodo} falhou (simulação)");
                    
                    // Redireciona de volta para a tela de pagamento específica
                    return RedirectToAction($"TelaPagamento{metodo}");
                }

                // Salvar dados do pagamento na sessão para a nota fiscal
                var pagamentoData = new PagamentoSessionData
                {
                    Metodo = metodo,
                    Subtotal = subtotal,
                    TotalFinal = totalFinal,
                    CupomAplicado = cupomData,
                    DataPagamento = DateTime.Now,
                    NumeroTransacao = Guid.NewGuid().ToString("N")[..8].ToUpper()
                };

                HttpContext.Session.SetObject("PagamentoData", pagamentoData);

                _logger.LogInformation($"Pagamento com {metodo} realizado com sucesso. Transação: {pagamentoData.NumeroTransacao}");
                TempData["Sucesso"] = "Pagamento realizado com sucesso!";
                
                return RedirectToAction("TelaNotaFiscal");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento");
                TempData["Erro"] = "Ocorreu um erro ao processar seu pagamento. Por favor, tente novamente.";
                return RedirectToAction("SelecionarPagamento");
            }
        }

        private PagamentoViewModel GetDadosPagamento()
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var cupomData = HttpContext.Session.GetObject<CupomSessionData>("CupomData");
            
            return new PagamentoViewModel
            {
                Cart = cart,
                CupomData = cupomData,
                TotalItens = cart.Sum(x => x.Quantity),
                Subtotal = cart.Sum(x => x.Price * x.Quantity),
                TotalFinal = cupomData?.TotalComDesconto ?? cart.Sum(x => x.Price * x.Quantity)
            };
        }
    }

    // ViewModel para dados de pagamento
    public class PagamentoViewModel
    {
        public List<CartItemViewModel> Cart { get; set; } = new List<CartItemViewModel>();
        public CupomSessionData? CupomData { get; set; }
        public int TotalItens { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalFinal { get; set; }
    }

    // Classe para dados do pagamento na sessão
    public class PagamentoSessionData
    {
        public string Metodo { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal TotalFinal { get; set; }
        public CupomSessionData? CupomAplicado { get; set; }
        public DateTime DataPagamento { get; set; }
        public string NumeroTransacao { get; set; } = string.Empty;
    }
}