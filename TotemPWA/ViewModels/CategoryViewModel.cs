using TotemPWA.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem

namespace TotemPWA.ViewModels
{
    public class CategoryViewModel
    {
        public Category Category { get; set; } = new Category(); // Inicializa para evitar null
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>(); // Para a lista drop-down de categorias pai
    }
}