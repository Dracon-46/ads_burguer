    using Microsoft.AspNetCore.Mvc.Rendering;
    using TotemPWA.Models;

    namespace TotemPWA.ViewModels
    {
        public class ProductViewModel
        {
            // Remova '= new Product();' ou '= default!;'
            // Apenas declare a propriedade:
            public Product Product { get; set; } = null!; // Adicionado '= null!;' para suprimir o warning CS8618 se aparecer.
                                                        // O Model Binder cuidará da inicialização.

            public List<SelectListItem>? Categories { get; set; }
        }
    }