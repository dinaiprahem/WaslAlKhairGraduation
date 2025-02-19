using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Authentication
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Full name must not exceed 50 characters.")]
        public string FullName { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public int? Age { get; set; }
        [Required]
        [RegularExpression(@"^(?:\+20|0)?(10|11|12|15)\d{8}$",
            ErrorMessage = "Invalid Egyptian phone number format.")]
        public string PhoneNumber { get; set; }
    }
}
