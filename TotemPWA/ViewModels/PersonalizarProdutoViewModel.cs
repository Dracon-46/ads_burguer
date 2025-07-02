using TotemPWA.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TotemPWA.ViewModels
{
    public class PersonalizarProdutoViewModel
    {
        public Product Produto { get; set; }
        public Guid CartItemId { get; set; } 
        public ICollection<Additional>? ProdutoAdditionals { get; set; }
        public Dictionary<int, int> QuantidadesManipuladas { get; set; } = new Dictionary<int, int>();
    }
}