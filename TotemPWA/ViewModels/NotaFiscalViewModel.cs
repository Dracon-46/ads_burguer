// Models/ViewModels/NotaFiscalViewModel.cs

using System;
using System.Collections.Generic;
using TotemPWA.Controllers; // Para acessar CupomSessionData
using TotemPWA.Models.ViewModels; // Para acessar CartItemViewModel

namespace TotemPWA.Models.ViewModels
{
    public class NotaFiscalViewModel
    {
        public string NomeComprador { get; set; }
        public string CPFComprador { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalFinal { get; set; }
        public decimal DescontoAplicado { get; set; }
        public string MetodoPagamento { get; set; }
        public string NumeroTransacao { get; set; }
        public DateTime DataPagamento { get; set; }
        public List<CartItemViewModel> Produtos { get; set; } = new List<CartItemViewModel>();
    }
}