using TotemPWA.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TotemPWA.ViewModels
{
    public class PersonalizarProdutoViewModel
    {
        public Product Produto { get; set; } = null!;
        public string TipoProduto { get; set; } = ""; // "Lanche", "Bebida", "Acompanhamento"
        public List<Ingredient> Ingredientes { get; set; } = new(); // para lanche
        public List<string> Tamanhos { get; set; } = new(); // para bebida ou acompanhamento
        public int OrderItemId { get; set; }
    }
}
