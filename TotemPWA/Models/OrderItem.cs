namespace TotemPWA.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public required Product Product { get; set; } // Adicionado 'required'

        public int OrderId { get; set; }
        public required Order Order { get; set; } // Adicionado 'required'

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public ICollection<Customize>? Customizations { get; set; } // Pode ser nulo
    }
}