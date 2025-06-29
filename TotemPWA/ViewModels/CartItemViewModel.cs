namespace TotemPWA.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; } // Preço unitário do item no carrinho, incluindo personalizações
        public string Image { get; set; } = "";
        public int Quantity { get; set; }

        // NOVAS PROPRIEDADES para personalização
        public Guid CartItemId { get; set; } // Um ID único para esta *instância* do item no carrinho

        public string? SelectedSize { get; set; } // Tamanho selecionado (para bebidas/acompanhamentos)
        public List<int> AddedIngredientIds { get; set; } = new List<int>(); // IDs dos ingredientes adicionados
        public List<int> RemovedIngredientIds { get; set; } = new List<int>(); // IDs dos ingredientes removidos
        public string? PersonalizationSummary { get; set; } // Resumo amigável da personalização
    }
}