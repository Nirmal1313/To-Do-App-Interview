using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Application.DTOs.RegistrationDto
{
    public class RegisterDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters long.")]
        [MaxLength(128)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z]).{10,}$", ErrorMessage = "Password criteria failed.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
