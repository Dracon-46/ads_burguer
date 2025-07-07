// TotemPWA\Models\ViewModels\ProductDisplayViewModel.cs
using System.Collections.Generic;

namespace TotemPWA.Models.ViewModels
{
    public class ProductDisplayViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; } // Preço com desconto (se houver promoção)
        public decimal? OriginalPrice { get; set; } // Preço original (antes do desconto)
        public string? ImageUrl { get; set; }

        public bool IsCombo { get; set; } = false;
        public List<IncludedProductViewModel>? ComboItems { get; set; }

        // Propriedades para promoções
        public bool HasPromotion { get; set; } = false;
        public decimal? PromotionPercent { get; set; }
    }
}