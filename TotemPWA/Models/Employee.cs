using System.ComponentModel.DataAnnotations;

namespace TotemPWA.Models
{
    public class Employee
    {
        [Key]
        public int ClientId { get; set; }
        public required Client Client { get; set; } // Adicionado 'required'

        public required string Type { get; set; }    // Adicionado 'required'
        public required string User { get; set; }    // Adicionado 'required'
        public required string Password { get; set; } // Adicionado 'required'
    }
}