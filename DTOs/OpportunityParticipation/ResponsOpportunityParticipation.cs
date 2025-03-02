using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.OpportunityParticipation
{
    public class ResponsOpportunityParticipation
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string NationalId { get; set; }
        public int Age { get; private set; }
        public string Gender { get; private set; }
        public string Specialization { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
