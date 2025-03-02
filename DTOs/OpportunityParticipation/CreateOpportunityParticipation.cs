using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.OpportunityParticipation
{
    public class CreateOpportunityParticipation
    {
        public string? AppUserId { get; set; }
        [Required]
        public int OpportunityId { get; set; }
        [Required]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        [RegularExpression(@"^(?:\+20|0)?(10|11|12|15)\d{8}$",
            ErrorMessage = "Invalid Egyptian phone number format.")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }
        [Required]
        public string NationalId { get; set; }
    }
}
