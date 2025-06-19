using TotemPWA.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem
using System.ComponentModel.DataAnnotations; // Para validações

namespace TotemPWA.ViewModels
{
    public class PromotionViewModel
    {
        public int Id { get; set; } // Para Edit

        [Required(ErrorMessage = "O produto é obrigatório.")]
        [Display(Name = "Produto")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "O percentual de desconto é obrigatório.")]
        [Range(0.01, 100.00, ErrorMessage = "O percentual deve ser entre 0.01 e 100.")]
        [Display(Name = "Percentual de Desconto")]
        public decimal Percent { get; set; }

        [Required(ErrorMessage = "A data de validade é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Válido Até")]
        public DateTime ValidUntil { get; set; }

        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>(); // Para o dropdown de produtos
    }
}