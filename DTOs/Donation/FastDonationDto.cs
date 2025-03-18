using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Donation
{
    // Input DTO for fast donation (distributed across all active opportunities)
    public class FastDonationDto
    {
        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }

        public bool IsGift { get; set; }

        public GiftDonationDto GiftDetails { get; set; }
    }
}
