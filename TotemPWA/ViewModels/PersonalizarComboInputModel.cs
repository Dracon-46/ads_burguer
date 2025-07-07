// TotemPWA\ViewModels\PersonalizarComboInputModel.cs
using System;
using System.Collections.Generic;

namespace TotemPWA.ViewModels
{
    public class PersonalizarComboInputModel
    {
        public int ComboProductId { get; set; } // ID do produto principal que representa o combo
        public Guid CartItemId { get; set; } // Para editar um item de combo existente no carrinho

        // Lista de personalizações para CADA ITEM DENTRO DO COMBO
        public List<ComboItemPersonalizationInputModel> ItemPersonalizations { get; set; } = new List<ComboItemPersonalizationInputModel>();
    }

    public class ComboItemPersonalizationInputModel
    {
        public int ProductId { get; set; } // ID do produto individual DENTRO do combo (ex: ID do Hambúrguer, ID da Batata)
        // Dicionário de ingredientes manipulados para ESTE ITEM ESPECÍFICO do combo
        public Dictionary<int, int> IngredientesManipuladasQuantidades { get; set; } = new Dictionary<int, int>();
    }
}