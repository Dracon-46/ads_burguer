// TotemPWA\ViewModels\ProductViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using TotemPWA.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq; 

namespace TotemPWA.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; } = new Product { Name = string.Empty };
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IFormFile? ImageFile { get; set; }
        // ProductViewModel.cs
        public List<SelectListItem>? AvailableIngredients { get; set; } // Para dropdown/lista de checkboxes de todos os ingredientes
        public List<Additional>? ProductAdditionals { get; set; } // Para manter as adições existentes para o produto sendo editado/criado
        // Isso manterá o estado de cada ingrediente (IsDefault, CanBeRemoved, CanBeAdded)
        public Dictionary<int, IngredientSelectionViewModel>? SelectedIngredients { get; set; }
    }
}