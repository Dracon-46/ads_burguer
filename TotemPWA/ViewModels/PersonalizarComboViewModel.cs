using TotemPWA.Models;

namespace TotemPWA.ViewModels
{
    public class PersonalizarComboViewModel
    {
        public Product ComboProduct { get; set; } // O produto que representa o combo
        public Guid CartItemId { get; set; }
        public List<ComboItemViewModel> ComboItems { get; set; } = new List<ComboItemViewModel>();
        public Dictionary<int, int> QuantidadesManipuladas { get; set; } = new Dictionary<int, int>();
        public decimal TotalPrice { get; set; }
    }
    
    public class ComboItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
        public ICollection<Additional>? ProductAdditionals { get; set; }
        public bool CanCustomize { get; set; } = true; // Define se o item do combo pode ser personalizado
    }
}