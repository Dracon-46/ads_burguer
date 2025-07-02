// Models/Additional.cs
using System.Text.Json.Serialization;

namespace TotemPWA.Models
{
    public class Additional
    {
        public int ProductId { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }

        public int IngredientId { get; set; }
        [JsonIgnore]
        public Ingredient? Ingredient { get; set; }

        // Removidas: IsDefault, CanBeRemoved, CanBeAdded
        public int Quantity { get; set; } = 1; // Mantida e agora crucial
    }
}