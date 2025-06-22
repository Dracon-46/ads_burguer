using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TotemPWA.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório.")]
        public int ProductId { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; } // Propriedade de navegação para o produto promovido

        [Required(ErrorMessage = "O percentual de desconto é obrigatório.")]
        [Column(TypeName = "decimal(5,2)")] // Ex: 15.00 para 15% (Permite até 999.99%)
        [Range(0.01, 100.00, ErrorMessage = "O percentual deve estar entre 0.01 e 100.")]
        public decimal Percent { get; set; }

        [Required(ErrorMessage = "A data de validade é obrigatória.")]
        [DataType(DataType.Date)] // Apenas a parte da data importa
        public DateTime ValidUntil { get; set; }
    }
}