namespace WaslAlkhair.Api.DTOs.Opportunity
{
    public class OpportunityDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Tasks { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int SeatsAvailable { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Benefits { get; set; } = string.Empty;
        public bool IsClosed { get; set; }
        public int RequiredAge { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public UserDto CreatedBy { get; set; } = null!;
        public List<ParticipationDto> Participants { get; set; } = new();
        public string CreatedById { get; set; } // User ID of the opportunity creator
    }

    public class UserDto
    {
        public string Role { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string image { get; set; }
    }

    public class ParticipationDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
