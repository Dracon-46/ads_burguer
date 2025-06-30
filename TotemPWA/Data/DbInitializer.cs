// DbInitializer.cs
using System.Text.Json;
using TotemPWA.Models; // Certifique-se de que este using está presente
using Microsoft.EntityFrameworkCore; // Adicione este using para métodos de EF Core como FirstOrDefaultAsync

namespace TotemPWA.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Verifica se já existem categorias (indicando que o banco já foi populado)
            if (context.Categories.Any())
                return;

            // Limpa o banco de dados antes de popular, para garantir um estado limpo para testes
            // CUIDADO: Não use isso em produção sem um backup ou confirmação
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync(); // Recria o schema com as últimas migrations

            var json = await File.ReadAllTextAsync("Data/SeedData.json");

            var rootCategories = JsonSerializer.Deserialize<List<CategorySeed>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Permite que "Additionals" no JSON mapeie para "Additionals" em C#
            });

            if (rootCategories != null)
            {
                // Lista para coletar todos os ingredientes distintos do SeedData para criar primeiro
                var allIngredientNames = new HashSet<string>();
                foreach (var categorySeed in rootCategories)
                {
                    CollectIngredientNamesRecursive(categorySeed, allIngredientNames);
                }

                // Cria ou garante que os ingredientes existam no banco de dados
                foreach (var ingredientName in allIngredientNames)
                {
                    if (!await context.Ingredients.AnyAsync(i => i.Name == ingredientName))
                    {
                        context.Ingredients.Add(new Ingredient { Name = ingredientName, Price = 0.00M, Limit = 999 }); // Defina um preço e limite padrão ou adicione ao JSON
                    }
                }
                await context.SaveChangesAsync();


                foreach (var categorySeed in rootCategories)
                {
                    await CreateCategoryRecursiveAsync(context, categorySeed, parentId: null);
                }

                await context.SaveChangesAsync();
            }
        }

        // NOVO: Método auxiliar para coletar todos os nomes de ingredientes
        private static void CollectIngredientNamesRecursive(CategorySeed seed, HashSet<string> ingredientNames)
        {
            if (seed.Products != null)
            {
                foreach (var productSeed in seed.Products)
                {
                    if (productSeed.Additionals != null)
                    {
                        foreach (var additionalSeed in productSeed.Additionals)
                        {
                            ingredientNames.Add(additionalSeed.IngredientName);
                        }
                    }
                }
            }
            if (seed.Subcategories != null)
            {
                foreach (var subcategorySeed in seed.Subcategories)
                {
                    CollectIngredientNamesRecursive(subcategorySeed, ingredientNames);
                }
            }
        }


        private static async Task CreateCategoryRecursiveAsync(ApplicationDbContext context, CategorySeed seed, int? parentId)
        {
            var category = new Category
            {
                Name = seed.Name,
                ParentCategoryId = parentId,
                // Assumindo que você tem um método para gerar slug se necessário, ou ele é automático
                // Se o slug não estiver vindo do JSON, você precisará gerá-lo aqui
                // Exemplo: Slug = GenerateSlug(seed.Name) ou deixar o modelo cuidar disso
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync(); // necessário para obter o Id da categoria

            // Cria os produtos da categoria
            foreach (var productSeed in seed.Products ?? new List<ProductSeed>())
            {
                var product = new Product
                {
                    Name = productSeed.Name,
                    Price = productSeed.Price,
                    Image = productSeed.Image,
                    CategoryId = category.Id,
                    Active = true // Define como ativo por padrão
                };

                context.Products.Add(product);
                await context.SaveChangesAsync(); // Necessário para obter o Id do produto antes de adicionar Additionals

                // NOVO: Adiciona os Additionals para o produto
                if (productSeed.Additionals != null && productSeed.Additionals.Any())
                {
                    foreach (var additionalSeed in productSeed.Additionals)
                    {
                        var ingredient = await context.Ingredients.FirstOrDefaultAsync(i => i.Name == additionalSeed.IngredientName);
                        if (ingredient != null)
                        {
                            context.Additionals.Add(new Additional
                            {
                                ProductId = product.Id,
                                IngredientId = ingredient.Id,
                                IsDefault = additionalSeed.IsDefault,
                                CanBeRemoved = additionalSeed.CanBeRemoved,
                                CanBeAdded = additionalSeed.CanBeAdded,
                                Price = additionalSeed.Price // Preço do adicional (se tiver)
                            });
                        }
                        else
                        {
                            // Opcional: Logar que um ingrediente não foi encontrado
                            Console.WriteLine($"Ingrediente '{additionalSeed.IngredientName}' não encontrado para o produto '{product.Name}'.");
                        }
                    }
                    await context.SaveChangesAsync(); // Salva os Additionals
                }
            }

            // Recursivamente cria subcategorias
            foreach (var subcategorySeed in seed.Subcategories ?? new List<CategorySeed>())
            {
                await CreateCategoryRecursiveAsync(context, subcategorySeed, category.Id);
            }
        }
    }
}