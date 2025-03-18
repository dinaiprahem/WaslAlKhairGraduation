using System.ComponentModel.DataAnnotations;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.Donation
{
    // Base donation DTO for displaying donation information
    public class DonationDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DonatedAt { get; set; }
        public string DonorName { get; set; }
        public DonationType Type { get; set; }
        public string CategoryName { get; set; }
        public List<DistributionDto> Distributions { get; set; }
        public GiftDonationDto GiftDonation { get; set; }
    }
}
