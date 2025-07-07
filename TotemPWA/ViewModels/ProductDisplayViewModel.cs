// TotemPWA\Models\ViewModels\ProductDisplayViewModel.cs
using System.Collections.Generic;

namespace TotemPWA.Models.ViewModels
{
    public class ProductDisplayViewModel // Este será o tipo dos objetos em ViewBag.Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        // Se a sua classe Product já tem um campo 'Image', você pode querer usar Image ou ImageUrl de forma consistente
        // public string? Image { get; set; }

        public bool IsCombo { get; set; } = false; // Indica se é um combo
        public List<IncludedProductViewModel>? ComboItems { get; set; } // Itens se for um combo
    }
}