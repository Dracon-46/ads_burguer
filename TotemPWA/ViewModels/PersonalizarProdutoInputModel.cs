using Microsoft.AspNetCore.Mvc.Rendering;
using TotemPWA.Models;
using System.Collections.Generic; 
public class PersonalizarProdutoInputModel
{
    public Guid CartItemId { get; set; } // ID do item no carrinho sendo editado 
    public int ProdutoId { get; set; }

    // Dicionário para enviar a quantidade de cada ingrediente manipulado.
    // A lógica se é uma adição ou remoção será baseada nas flags IsDefault/CanBeRemoved/CanBeAdded do Additional correspondente.
    // Chave: IngredientId, Valor: Quantidade final do ingrediente após a manipulação.
    public Dictionary<int, int> IngredientesManipuladosQuantidades { get; set; } = new Dictionary<int, int>();
}