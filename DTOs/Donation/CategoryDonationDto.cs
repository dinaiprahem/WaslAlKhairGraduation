using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Donation
{
    // Input DTO for donating to a category
    public class CategoryDonationDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }

        public bool IsGift { get; set; }

        public GiftDonationDto GiftDetails { get; set; }
    }
}
