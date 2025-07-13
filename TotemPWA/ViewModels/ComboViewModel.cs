using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TotemPWA.Models.ViewModels
{
    public class ComboViewModel
    {
        public int ProductComboId { get; set; }

        [Required(ErrorMessage = "O nome do combo é obrigatório.")]
        [Display(Name = "Nome do Combo")]
        public string ComboProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preço do combo é obrigatório.")]
        [Display(Name = "Preço do Combo")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        public decimal ComboPrice { get; set; }

        [Display(Name = "Descrição do Combo")]
        public string? ComboDescription { get; set; }

        [Display(Name = "Imagem do Combo")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "URL da Imagem")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Selecione os produtos que compõem o combo.")]
        [Display(Name = "Itens do Combo")]
        public List<int> SelectedProductIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? AvailableProducts { get; set; }

        public List<IncludedProductViewModel> IncludedProducts { get; set; } = new List<IncludedProductViewModel>();

        // Propriedades para indicar se é criação ou edição
        public bool IsEdit { get; set; } = false;
        
        // Preço calculado automaticamente (soma dos produtos) - apenas para referência
        public decimal CalculatedPrice { get; set; }
    }

    public class IncludedProductViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal ProductPrice { get; set; }
    }
}