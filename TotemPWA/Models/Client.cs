namespace TotemPWA.Models
{
    public class Client
    {
        public int Id { get; set; }
        public required string Name { get; set; } // Adicionado 'required'
        public required string CPF { get; set; }  // Adicionado 'required'

        public Employee? Employee { get; set; } // Pode ser nulo
        public ICollection<Order>? Orders { get; set; } // Pode ser nulo, ajustado para '?'
    }
}