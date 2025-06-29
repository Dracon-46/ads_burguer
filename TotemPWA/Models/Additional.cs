using System.Text.Json.Serialization;

namespace TotemPWA.Models
{
    public class Additional
    {
        // Propriedades existentes
        public int ProductId { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }

        public int IngredientId { get; set; }
        [JsonIgnore]
        public Ingredient? Ingredient { get; set; }

        // NOVAS PROPRIEDADES para personalização
        public bool IsDefault { get; set; } // Indica se este ingrediente vem por padrão com o produto
        public bool CanBeRemoved { get; set; } // Indica se este ingrediente padrão pode ser removido pelo cliente
        public bool CanBeAdded { get; set; } // Indica se este ingrediente pode ser adicionado (ex: extra)
        public decimal Price { get; set; } // Preço para adicionar (se CanBeAdded for true e tiver custo extra)
    }
}