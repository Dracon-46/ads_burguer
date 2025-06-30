// Models/Customize.cs
namespace TotemPWA.Models
{
    public class Customize
    {
        public int Id { get; set; } // Id da customização em si pode continuar int

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public Guid OrderItemId { get; set; } // <<-- MUDANÇA AQUI!
        public OrderItem? OrderItem { get; set; }

        public string Type { get; set; } = "";
    }
}