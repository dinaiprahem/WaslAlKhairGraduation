using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.User
{
    public class updateUserDTO
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public int Age { get; set; }
        public IFormFile? Image { get; set; }
    }
}
