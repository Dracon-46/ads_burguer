// ClassSeed.cs
public class CategorySeed
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<CategorySeed>? Subcategories { get; set; }
    public List<ProductSeed>? Products { get; set; }
}

public class ProductSeed
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } 
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; } 
    public List<AdditionalSeed>? Additionals { get; set; }
}

public class AdditionalSeed
{
    public string IngredientName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0; 
}
public class IngredientSeed
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; } = 3m; // Preço base do ingrediente
    public int Limit { get; set; } = 10; // Limite padrão de quantas vezes pode ser adicionado pelo cliente
}