namespace WaslAlkhair.Api.DTOs.Opportunity
{
    public class OpportunitySearchDto
    {
        public string? Location { get; set; }
        public string? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinAge { get; set; }
        public int? MaxSeats { get; set; }
        public bool? IsOpen { get; set; }
        public string? SearchTerm { get; set; }
    }
    public class OpportunitySearchParams
    {
        public string? Location { get; set; }
        public string? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinAge { get; set; }
        public int? MaxSeats { get; set; }
        public bool? IsOpen { get; set; }
        public string? SearchTerm { get; set; }
    }
}
