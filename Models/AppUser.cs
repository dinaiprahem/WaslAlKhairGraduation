using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WaslAlkhair.Api.Models
{
	public class AppUser : IdentityUser
	{
		// Common properties for all users
		[Required, MaxLength(50)]
		public string FullName { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// User and Charity Common properties
		public DateOnly DateOfBirth { get; set; } // Creation Date for Charity, DateOfBirth for User

		// Charity-specific properties
		public string? CharityRegistrationNumber { get; set; }
		public string? CharityMission { get; set; }
		public string? Address { get; set; }

		//  (One AppUser -> Many OpportunityParticipations)
		public ICollection<OpportunityParticipation> OpportunityParticipations { get; set; } = new List<OpportunityParticipation>();
		// created by the user/charity
		public ICollection<Opportunity> CreatedOpportunities { get; set; } = new List<Opportunity>();

	}
}
