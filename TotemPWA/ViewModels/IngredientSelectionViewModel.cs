// TotemPWA\ViewModels\IngredientSelectionViewModel.cs
namespace TotemPWA.ViewModels
{
    public class IngredientSelectionViewModel
    {
        public int IngredientId { get; set; }
        public string? IngredientName { get; set; }
        public bool IsSelected { get; set; } // Se o ingrediente está associado ao produto
        // Removidas: IsDefault, CanBeRemoved, CanBeAdded
        public decimal Price { get; set; } // Preço adicional do ingrediente (ainda pode ser útil para exibição)
        public int Limit { get; set; } // Limite de quantidade para adição (ainda pode ser útil para exibição no cliente)
        public int Quantity { get; set; } // Quantidade definida no CRUD para este ingrediente no produto
    }
}