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
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Slug = GenerateSlug(value);
            }
        }

        public string Slug { get; private set; } = string.Empty;


        public int? ParentCategoryId { get; set; }

        [JsonIgnore]
        public Category? ParentCategory { get; set; }

        [JsonIgnore]
        public ICollection<Category> Subcategories { get; set; } = new List<Category>();

        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();

        private static string GenerateSlug(string text)
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