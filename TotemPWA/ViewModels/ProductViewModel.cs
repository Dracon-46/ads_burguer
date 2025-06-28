// TotemPWA\ViewModels\ProductViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using TotemPWA.Models;
using Microsoft.AspNetCore.Http;

namespace TotemPWA.ViewModels
{
    public class ProductViewModel
    {
        // CORREÇÃO AQUI: Inicializando Name para satisfazer o 'required'
        public Product Product { get; set; } = new Product { Name = string.Empty }; // <<-- LINHA CORRIGIDA
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}