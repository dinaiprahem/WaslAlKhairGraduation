using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Authentication
{
    public class RegisterRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string Major { get; set; }
        [Required]
        public int? Age { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}
