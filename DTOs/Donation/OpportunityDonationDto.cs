using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Donation
{
    // Input DTO for donating to a specific opportunity
    public class OpportunityDonationDto
    {
        [Required]
        public int OpportunityId { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }

        public bool IsGift { get; set; }

        public GiftDonationDto GiftDetails { get; set; }
    }
}
