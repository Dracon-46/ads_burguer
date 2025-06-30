using TotemPWA.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TotemPWA.ViewModels
{
    public class PersonalizarProdutoViewModel
    {
        public Product Produto { get; set; } = null!;
        public string TipoProduto { get; set; } = "";
        public List<Ingredient> IngredientesDisponiveis { get; set; } = new();
        public List<Ingredient> IngredientesPadrao { get; set; } = new();
        public List<string> TamanhosDisponiveis { get; set; } = new();

        public string? TamanhoAtual { get; set; }
        public List<int> IngredientesAtuaisAdicionados { get; set; } = new();
        public List<int> IngredientesAtuaisRemovidos { get; set; } = new();
        public Guid CartItemId { get; set; } 
    }
}
