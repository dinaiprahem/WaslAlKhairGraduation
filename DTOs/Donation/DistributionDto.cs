namespace WaslAlkhair.Api.DTOs.Donation
{    // DTO for distribution information
    public class DistributionDto
    {
        public int OpportunityId { get; set; }
        public string OpportunityTitle { get; set; }
        public decimal Amount { get; set; }
    }
}
