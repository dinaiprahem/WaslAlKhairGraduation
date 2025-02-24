using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Charity
{
    public class UpdateCharityDTO
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }
        [Required]
        public string? CharityName { get; set; }
        [Required]
        public string? CharityRegistrationNumber { get; set; }
        [Required]
        public string? CharityMission { get; set; }
        [Required]
        public DateOnly? EstablishedAt { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public IFormFile? Image { get; set; }
    }
}
