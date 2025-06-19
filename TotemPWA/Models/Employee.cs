using System.ComponentModel.DataAnnotations;

namespace TotemPWA.Models
{
    public class Employee
    {
        [Key]
        public int ClientId { get; set; }

        // Mude Client para Client? (tornando-o anulável), pois é uma propriedade de navegação
        // e pode não estar carregada/presente ao criar um Employee.
        public Client? Client { get; set; }

        // Inicialize as propriedades de string com string.Empty para satisfazer o compilador,
        // já que elas são não-anuláveis (sem '?')
        public string Type { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}