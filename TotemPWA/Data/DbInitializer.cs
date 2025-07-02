// DbInitializer.cs
using System.Text.Json;
using TotemPWA.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Adicione este using
using System.Collections.Generic; // Adicione este using para HashSet e List

namespace TotemPWA.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            if (context.Categories.Any())
                return;

            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();

            var json = await File.ReadAllTextAsync("Data/SeedData.json");

            var rootCategories = JsonSerializer.Deserialize<List<CategorySeed>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Permite que "Additionals" no JSON mapeie para "Additionals" em C#
            });

            if (rootCategories != null)
            {
                // Lista para coletar todos os ingredientes distintos e seus preços do SeedData para criar primeiro
                var allIngredientDetails = new Dictionary<string, decimal>(); // Nome -> Preço (se houver no SeedData.json)
                CollectIngredientDetailsRecursive(rootCategories, allIngredientDetails);

                // Ingredientes base (alguns exemplos com preços e limites)
                // É importante que esses ingredientes existam com um preço para que o cálculo de personalização funcione
                var initialIngredients = new List<Ingredient>
                {
                    new Ingredient { Name = "Pão de Brioche", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Carne de 180g", Price = 0.00M, Limit = 3 },
                    new Ingredient { Name = "Queijo Cheddar", Price = 2.00M, Limit = 5 },
                    new Ingredient { Name = "Alface", Price = 0.50M, Limit = 3 },
                    new Ingredient { Name = "Tomate", Price = 0.50M, Limit = 3 },
                    new Ingredient { Name = "Cebola Roxa", Price = 0.75M, Limit = 3 },
                    new Ingredient { Name = "Molho Especial", Price = 1.50M, Limit = 3 },
                    new Ingredient { Name = "Bacon Crocante", Price = 3.00M, Limit = 4 },
                    new Ingredient { Name = "Ovo Frito", Price = 2.50M, Limit = 2 },
                    new Ingredient { Name = "Pão Australiano", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Carne de 200g", Price = 0.00M, Limit = 3 },
                    new Ingredient { Name = "Queijo Provolone", Price = 2.50M, Limit = 5 },
                    new Ingredient { Name = "Rúcula", Price = 0.50M, Limit = 3 },
                    new Ingredient { Name = "Cebola Caramelizada", Price = 1.00M, Limit = 3 },
                    new Ingredient { Name = "Pão de Gergelim", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Carne de 100g", Price = 0.00M, Limit = 4 },
                    new Ingredient { Name = "Queijo Americano", Price = 1.80M, Limit = 5 },
                    new Ingredient { Name = "Picles", Price = 0.50M, Limit = 4 },
                    new Ingredient { Name = "Carne de 150g", Price = 0.00M, Limit = 3 },
                    new Ingredient { Name = "Queijo Suíço", Price = 2.20M, Limit = 5 },
                    new Ingredient { Name = "Molho Aioli", Price = 1.80M, Limit = 3 },
                    new Ingredient { Name = "Pão Integral", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Frango Grelhado", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Queijo Minas", Price = 1.70M, Limit = 5 },
                    new Ingredient { Name = "Espinafre", Price = 0.60M, Limit = 3 },
                    new Ingredient { Name = "Tomate Seco", Price = 1.20M, Limit = 3 },
                    new Ingredient { Name = "Pão de Hot Dog", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Salsicha", Price = 0.00M, Limit = 3 },
                    new Ingredient { Name = "Maionese", Price = 0.50M, Limit = 3 },
                    new Ingredient { Name = "Ketchup", Price = 0.50M, Limit = 3 },
                    new Ingredient { Name = "Mostarda", Price = 0.50M, Limit = 3 },
                    new Ingredient { Name = "Batata Palha", Price = 1.00M, Limit = 3 },
                    new Ingredient { Name = "Molho Barbecue", Price = 0.80M, Limit = 3 },
                    new Ingredient { Name = "Cebola Crispy", Price = 1.20M, Limit = 3 },
                    new Ingredient { Name = "Purê de Batata", Price = 1.50M, Limit = 2 },
                    new Ingredient { Name = "Vinagrete", Price = 0.70M, Limit = 2 },
                    new Ingredient { Name = "Queijo Ralado", Price = 0.90M, Limit = 3 },
                    new Ingredient { Name = "Alface Americana", Price = 0.40M, Limit = 3 },
                    new Ingredient { Name = "Tomate Cereja", Price = 0.60M, Limit = 3 },
                    new Ingredient { Name = "Pepino", Price = 0.40M, Limit = 3 },
                    new Ingredient { Name = "Grão de Bico", Price = 1.00M, Limit = 2 },
                    new Ingredient { Name = "Molho Balsâmico", Price = 1.00M, Limit = 2 },
                    new Ingredient { Name = "Manga", Price = 1.50M, Limit = 2 },
                    new Ingredient { Name = "Castanha de Caju", Price = 2.00M, Limit = 2 },
                    new Ingredient { Name = "Molho de Maracujá", Price = 1.20M, Limit = 2 },
                    new Ingredient { Name = "Frango Desfiado", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Milho", Price = 0.30M, Limit = 3 },
                    new Ingredient { Name = "Queijo Coalho", Price = 2.50M, Limit = 3 },
                    new Ingredient { Name = "Croutons", Price = 0.80M, Limit = 3 },
                    new Ingredient { Name = "Molho Caesar", Price = 1.00M, Limit = 2 },
                    new Ingredient { Name = "Wrap Integral", Price = 0.00M, Limit = 1 },
                    new Ingredient { Name = "Peito de Peru", Price = 0.00M, Limit = 2 },
                    new Ingredient { Name = "Cream Cheese", Price = 1.00M, Limit = 2 },
                    new Ingredient { Name = "Cenoura Ralada", Price = 0.30M, Limit = 3 },
                    new Ingredient { Name = "Pão de Forma", Price = 0.00M, Limit = 4 },
                    new Ingredient { Name = "Queijo Mussarela", Price = 1.80M, Limit = 4 },
                    new Ingredient { Name = "Queijo Prato", Price = 1.90M, Limit = 4 },
                    new Ingredient { Name = "Orégano", Price = 0.20M, Limit = 2 },
                    new Ingredient { Name = "Requeijão", Price = 0.90M, Limit = 2 }
                };

                foreach (var ing in initialIngredients)
                {
                    if (!await context.Ingredients.AnyAsync(i => i.Name == ing.Name))
                    {
                        context.Ingredients.Add(ing);
                    }
                }
                await context.SaveChangesAsync();

                // Agora, mapeie os ingredientes do SeedData.json para os IDs reais do banco de dados
                // e crie os objetos Category e Product
                foreach (var categorySeed in rootCategories)
                {
                    await CreateCategoryRecursiveAsync(context, categorySeed, parentId: null);
                }

                await context.SaveChangesAsync();
            }
        }

        // NOVO: Método auxiliar para coletar todos os nomes de ingredientes
        private static void CollectIngredientDetailsRecursive(List<CategorySeed> categorySeeds, Dictionary<string, decimal> ingredientDetails)
        {
            foreach (var categorySeed in categorySeeds)
            {
                if (categorySeed.Products != null)
                {
                    foreach (var productSeed in categorySeed.Products)
                    {
                        if (productSeed.Additionals != null)
                        {
                            foreach (var additionalSeed in productSeed.Additionals)
                            {
                                // A lógica para obter o preço do ingrediente no SeedData.json foi removida
                                // porque agora o AdditionalSeed não tem campo de preço.
                                // O preço será puxado do Ingredient que já foi seedado/existe no DB.
                                if (!ingredientDetails.ContainsKey(additionalSeed.IngredientName))
                                {
                                    ingredientDetails.Add(additionalSeed.IngredientName, 0.00M); // Preço placeholder, será atualizado pelo DB
                                }
                            }
                        }
                    }
                }
                if (categorySeed.Subcategories != null)
                {
                    CollectIngredientDetailsRecursive(categorySeed.Subcategories, ingredientDetails);
                }
            }
        }

        private static async Task CreateCategoryRecursiveAsync(ApplicationDbContext context, CategorySeed seed, int? parentId)
        {
            var category = new Category
            {
                Name = seed.Name,
                ParentCategoryId = parentId,
                Slug = GenerateSlug(seed.Name), // Gerar slug aqui
                Icon = seed.Icon // Usar o ícone do seed
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            foreach (var productSeed in seed.Products ?? new List<ProductSeed>())
            {
                var product = new Product
                {
                    Name = productSeed.Name,
                    Description = productSeed.Description, // Inclui descrição
                    Price = productSeed.Price,
                    ImageUrl = productSeed.ImageUrl, // Inclui ImageUrl
                    CategoryId = category.Id,
                    Active = true
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

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
                                Quantity = additionalSeed.Quantity // <<-- USA A QUANTIDADE DO SEEDDATA
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Ingrediente '{additionalSeed.IngredientName}' não encontrado para o produto '{product.Name}'. Verifique a lista de Ingredients base.");
                        }
                    }
                    await context.SaveChangesAsync();
                }
            }

            foreach (var subcategorySeed in seed.Subcategories ?? new List<CategorySeed>())
            {
                await CreateCategoryRecursiveAsync(context, subcategorySeed, category.Id);
            }
        }

        // Método simples para gerar slug
        private static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLowerInvariant();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}