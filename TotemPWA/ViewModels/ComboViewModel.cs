using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TotemPWA.Models.ViewModels
{
    public class ComboViewModel
    {
        public int ProductComboId { get; set; }

        [Display(Name = "Nome do Combo")]
        public string? ComboProductName { get; set; }

        [Display(Name = "Preço Total do Combo")]
        public decimal ComboPrice { get; set; }

        [Required(ErrorMessage = "Selecione os produtos que compõem o combo.")]
        [Display(Name = "Itens do Combo")]
        public List<int> SelectedProductIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? AvailableProducts { get; set; }

        public List<IncludedProductViewModel> IncludedProducts { get; set; } = new List<IncludedProductViewModel>();
    }

    public class IncludedProductViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal ProductPrice { get; set; }
    }
}