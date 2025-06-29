// Models/OrderItem.cs
namespace TotemPWA.Models
{
    public class OrderItem
    {
        // Mude de 'int Id' para 'Guid Id'
        public Guid Id { get; set; } // <<-- MUDANÇA AQUI!

        public int ProductId { get; set; }
        public required Product Product { get; set; }

        public int OrderId { get; set; } // O Id da Order continua sendo int
        public required Order Order { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public ICollection<Customize>? Customizations { get; set; }

        public string? SelectedSize { get; set; }
        public string? PersonalizationSummary { get; set; }
    }
}