using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.Models
{
    public class DonationDistribution
    {
        // Composite key: DonationId + DonationOpportunityId

        public int DonationId { get; set; }
        public Donation Donation { get; set; }

        public int DonationOpportunityId { get; set; }
        public DonationOpportunity DonationOpportunity { get; set; }

        [Required]
        public decimal DistributedAmount { get; set; }

    }
}
