namespace WaslAlkhair.Api.DTOs.HomeSearch
{
    public class SearchResultDto
    {
        public List<SearchItemDto> Opportunities { get; set; } = new List<SearchItemDto>();
        public List<SearchItemDto> Donations { get; set; } = new List<SearchItemDto>();
        public List<SearchItemDto> Assistances { get; set; } = new List<SearchItemDto>();
        public List<SearchItemDto> AssistanceTypes { get; set; } = new List<SearchItemDto>();
        public List<SearchItemDto> Charities { get; set; } = new List<SearchItemDto>();
        public List<SearchItemDto> DonationCategories { get; set; } = new List<SearchItemDto>();
        public List<SearchItemDto> Users { get; set; } = new List<SearchItemDto>();
    }

    public class SearchItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
    }
} 