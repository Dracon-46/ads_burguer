namespace TotemPWA.Models
{
    public class Customize
    {
        public int Id { get; set; }

        public int IngredientId { get; set; }
        public required Ingredient Ingredient { get; set; } // Adicionado 'required'

        public int OrderItemId { get; set; }
        public required OrderItem OrderItem { get; set; } // Adicionado 'required'

        public required string Type { get; set; } // Adicionado 'required' (adicionar ou remover)
    }
}