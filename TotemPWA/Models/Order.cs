namespace TotemPWA.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public required Client Client { get; set; } // Adicionado 'required'

        public int? CupomId { get; set; }
        public Cupom? Cupom { get; set; } // Pode ser nulo

        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }
        public required string Status { get; set; } // Adicionado 'required'

        public ICollection<OrderItem>? Items { get; set; } // Pode ser nulo
        public ICollection<Payment>? Payments { get; set; } // Pode ser nulo
    }
}