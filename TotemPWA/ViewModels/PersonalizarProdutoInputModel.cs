using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System; 

namespace TotemPWA.ViewModels
{
    public class PersonalizarProdutoInputModel
    {
        public int ProdutoId { get; set; }
        public Guid CartItemId { get; set; } 
   
        public Dictionary<int, int> IngredientesManipuladasQuantidades { get; set; } = new Dictionary<int, int>(); // <<-- CORREÇÃO AQUI!
    }
}
