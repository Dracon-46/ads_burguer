namespace TotemPWA.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public required Order Order { get; set; } // Adicionado 'required'
        public decimal Amount { get; set; }
        public required string PaymentMethod { get; set; } // Adicionado 'required'
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; } // Adicionado '?' (pode ser nulo)
        public required string Status { get; set; } // Adicionado 'required'
    }
}