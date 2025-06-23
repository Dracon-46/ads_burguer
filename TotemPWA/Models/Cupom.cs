namespace TotemPWA.Models
{
    public class Cupom
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string Type { get; set; } // Adicionado 'required'
        public decimal Value { get; set; }

        public ICollection<Order>? Orders { get; set; }
    }
}