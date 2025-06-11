namespace WaslAlkhair.Api.DTOs.Donation
{
    public class UpdateDonationOpportunityDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        public decimal? TargetAmount { get; set; }
        public DateTime? Deadline { get; set; }
    }

}
