// ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace TotemPWA.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        [Display(Name = "Usuário")]
        public string User { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }
    }
}