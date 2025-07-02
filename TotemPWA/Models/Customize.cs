// Models/Customize.cs
namespace TotemPWA.Models
{
    public class Customize
    {
        public int Id { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public Guid OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        public string Type { get; set; } = "";
    }
}