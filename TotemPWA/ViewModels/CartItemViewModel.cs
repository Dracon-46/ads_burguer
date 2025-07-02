namespace TotemPWA.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; } // Preço unitário do item no carrinho, incluindo personalizações
        public string Image { get; set; } = "";
        public int Quantity { get; set; }

        public Guid CartItemId { get; set; } // Um ID único para esta *instância* do item no carrinho

        // Dicionário para armazenar a quantidade de cada ingrediente manipulado nesta personalização
        // Chave: IngredientId, Valor: Quantidade final do ingrediente
        public Dictionary<int, int> ManipulatedIngredientsWithQuantity { get; set; } = new Dictionary<int, int>();

        public string? PersonalizationSummary { get; set; } // Resumo amigável da personalização
    }
}