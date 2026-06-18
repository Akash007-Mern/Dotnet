using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Dotnet.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string UserPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("UserPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string CurrentAddress { get; set; } = null!;

        [DataType(DataType.Upload)]
        public IFormFile? UserFile { get; set; }

        public string UserRole { get; set; } = "User";
    }
}
