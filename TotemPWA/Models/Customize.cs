namespace TotemPWA.Models
{
    public class Customize
    {
        public int Id { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; } // Navegação opcional

        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; } // Navegação opcional

        public string Type { get; set; } = ""; // Já inicializada, sem 'required'
    }
}
