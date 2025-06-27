using Microsoft.AspNetCore.Mvc.Rendering;
using TotemPWA.Models;


public class PersonalizarProdutoInputModel
{
    public int OrderItemId { get; set; }
    public int ProdutoId { get; set; }
    public string TipoProduto { get; set; } = string.Empty;

    public List<int> IngredientesParaAdicionar { get; set; } = new();
    public List<int> IngredientesParaRemover { get; set; } = new();

    public string? TamanhoSelecionado { get; set; }
}
