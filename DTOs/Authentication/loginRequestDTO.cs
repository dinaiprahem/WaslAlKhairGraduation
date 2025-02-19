using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Authentication
{
    public class loginRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
