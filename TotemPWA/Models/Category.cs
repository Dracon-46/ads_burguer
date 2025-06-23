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

<<<<<<< .merge_file_e0MPn5
        private string _name = string.Empty;
=======
        private string _name;

        public string Slug { get; private set; } = string.Empty;
>>>>>>> .merge_file_yP8GHD

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Slug = GenerateSlug(value);
            }
        }

<<<<<<< .merge_file_e0MPn5
        public string Slug { get; private set; } = string.Empty;
=======
        public required string Icon { get; set; }
        public int? ParentCategoryId { get; set; }

        [JsonIgnore]
        public Category? ParentCategory { get; set; }

        public ICollection<Category> Subcategories { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();

        private string RemoveDiacritics(string text)
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

>>>>>>> .merge_file_yP8GHD

        [Required(ErrorMessage = "O ícone da categoria é obrigatório.")]
        public string Icon { get; set; } = string.Empty; // Inicializa para evitar null

        public int? ParentCategoryId { get; set; } // Pode ser nulo para categorias pai

        [JsonIgnore] // Evita loop de serialização JSON
        public Category? ParentCategory { get; set; } // Propriedade de navegação para a categoria pai

        [JsonIgnore] // Evita loop de serialização JSON
        public ICollection<Category> Subcategories { get; set; } = new List<Category>();

        [JsonIgnore] // Evita loop de serialização JSON
        public ICollection<Product> Products { get; set; } = new List<Product>();

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Transforma para minúsculas e remove espaços extras
            text = text.ToLowerInvariant().Trim();

            // Remove acentos (ex: ç, ã, é → c, a, e)
            text = RemoveDiacritics(text);

            // Remove caracteres inválidos
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

            // Substitui espaços por hífens
            text = Regex.Replace(text, @"\s+", "-");

            // Remove múltiplos hífens
            text = Regex.Replace(text, @"-+", "-");
<<<<<<< .merge_file_e0MPn5
            return text;
        }
=======

            // Remove hífens no início/fim
            text = text.Trim('-');

            return text;
        }

>>>>>>> .merge_file_yP8GHD
    }
}