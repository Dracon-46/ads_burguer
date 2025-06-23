using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TotemPWA.Models
{
    public class Combo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O produto combo é obrigatório.")]
        public int ProductComboId { get; set; }
        [JsonIgnore]
        public Product? ProductCombo { get; set; } // O produto que é um combo

        [Required(ErrorMessage = "O produto individual é obrigatório.")]
        public int ProductId { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; } // Os produtos que compõem o combo
    }
}