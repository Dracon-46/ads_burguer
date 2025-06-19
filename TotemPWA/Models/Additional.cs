using System.Text.Json.Serialization;

namespace TotemPWA.Models
{
    public class Additional
    {
        public int ProductId { get; set; }
        [JsonIgnore] // Evita loop de serialização JSON
        public Product? Product { get; set; } // Propriedade de navegação nula

        public int IngredientId { get; set; }
        [JsonIgnore] // Evita loop de serialização JSON
        public Ingredient? Ingredient { get; set; } // Propriedade de navegação nula
    }
}