    // Exemplo de TotemPWA\ViewModels\CategoryViewModel.cs
    // (Ajuste conforme as propriedades que você precisa para o formulário)
    using System.ComponentModel.DataAnnotations;

    namespace TotemPWA.ViewModels
    {
        public class CategoryViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
            [StringLength(100, ErrorMessage = "O nome da categoria não pode exceder 100 caracteres.")]
            public string Name { get; set; } = string.Empty; // Inicialize para evitar avisos de nulidade

            [StringLength(255, ErrorMessage = "A descrição não pode exceder 255 caracteres.")]
            public string? Description { get; set; }

            public int? ParentCategoryId { get; set; }

            // Você pode adicionar outras propriedades aqui que são úteis APENAS para a view,
            // como uma lista de categorias pai para o Dropdown
            public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? ParentCategoryOptions { get; set; }
        }
    }