using System.ComponentModel.DataAnnotations;

namespace PL.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Пароль має містити щонайменше 6 символів.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}
