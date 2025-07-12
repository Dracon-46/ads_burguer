// TotemPWA/ViewModels/PersonalizarProdutoViewModel.cs
using TotemPWA.Models;
using System.Collections.Generic;

namespace TotemPWA.ViewModels
{
    public class PersonalizarProdutoViewModel
    {
        public Product Produto { get; set; }
        public Guid CartItemId { get; set; } 
        public ICollection<Additional>? ProdutoAdditionals { get; set; }
        public Dictionary<int, int> QuantidadesManipuladas { get; set; } = new Dictionary<int, int>();

        // NOVO: Adicionar o preço base calculado (considerando promoções)
        public decimal BasePriceWithPromotion { get; set; }
    }
}