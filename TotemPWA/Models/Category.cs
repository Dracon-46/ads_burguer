using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace TotemPWA.Models
{
    public class Category
    {
        public int Id { get; set; }

        private string _name = string.Empty;

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da categoria não pode exceder 100 caracteres.")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value ?? string.Empty; // Garante que _name nunca seja nulo
                Slug = GenerateSlug(_name);     // O Slug é gerado automaticamente aqui!
            }
        }

         // Mudar para 'public set;' para permitir atribuição externa
        public string Slug { get; set; } = string.Empty; 

        // ADICIONAR ESTA PROPRIEDADE
        public string? Icon { get; set; } 

        [StringLength(255, ErrorMessage = "A descrição não pode exceder 255 caracteres.")]
        public string? Description { get; set; } // Pode ser nulo

        public int? ParentCategoryId { get; set; } // Pode ser nulo (para categorias principais)

        [JsonIgnore]
        public Category? ParentCategory { get; set; } // Pode ser nulo, por isso o '?'

        [JsonIgnore]
        public ICollection<Category>? Subcategories { get; set; } = new List<Category>(); // Inicializado para evitar null

        [JsonIgnore]
        public ICollection<Product>? Products { get; set; } = new List<Product>(); // Inicializado para evitar null

        // Importante: Remova 'static' aqui para que seja um método de instância
        private string GenerateSlug(string text) 
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.ToLowerInvariant().Trim();
            text = RemoveDiacritics(text); 
            text = Regex.Replace(text, @"[^a-z0-9\s-]", ""); 
            text = Regex.Replace(text, @"\s+", "-");        
            text = Regex.Replace(text, @"-+", "-");         

            return text.Trim('-');
        }

        // Este método auxiliar pode permanecer estático, pois não depende da instância da Category
        private static string RemoveDiacritics(string text) 
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}