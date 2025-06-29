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
    public string Image { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // NOVO: Lista de Additionals para este produto
    public List<AdditionalSeed>? Additionals { get; set; }
}

// NOVO: Classe para representar um adicional no SeedData.json
public class AdditionalSeed
{
    public string IngredientName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool CanBeRemoved { get; set; }
    public bool CanBeAdded { get; set; }
    public decimal Price { get; set; } // Preço do adicional se CanBeAdded for true
}

// NOVO: Classe para representar um ingrediente simples no SeedData.json
public class IngredientSeed
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0m; // Preço base do ingrediente (se ele puder ser vendido separado ou ter um custo intrínseco)
    public int Limit { get; set; } = 1; // Limite padrão
}