using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Donation
{
    // DTO for gift donation information
    public class GiftDonationDto
    {
        [Required]
        public string RecipientName { get; set; }

        [Required]
        public string RecipientPhone { get; set; }

        public bool ShowAmount { get; set; }
        public bool ShowOpportunity { get; set; }
    }
}
