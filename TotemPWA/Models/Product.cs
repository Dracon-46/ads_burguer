namespace TotemPWA.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; } // Pode ser nulo
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? Image { get; set; }

        // ADICIONE ESTA LINHA:
        public bool Active { get; set; } = true; // Define como true por padrão para novos produtos.

        public ICollection<Additional>? Additionals { get; set; }
        public ICollection<Combo>? ProductCombos { get; set; }
        public ICollection<Combo>? ComposedCombos { get; set; }
        public ICollection<Promotion>? Promotions { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}