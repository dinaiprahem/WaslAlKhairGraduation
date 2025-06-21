using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using static Twilio.Rest.Content.V1.ContentResource;

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
        public string? image { get; set; }

        // Charity-specific properties
        public string? CharityRegistrationNumber { get; set; }
		public string? CharityMission { get; set; }
		public string? Address { get; set; }

		//  (One AppUser -> Many OpportunityParticipations)
		public ICollection<OpportunityParticipation> OpportunityParticipations { get; set; } = new List<OpportunityParticipation>();
		// created by the user/charity
		public ICollection<Opportunity> CreatedOpportunities { get; set; } = new List<Opportunity>();
		//every user can create many assistances
		public ICollection<Assistance> Assistances { get; set; } = new List<Assistance>();

        public ICollection<UserReview> ReviewsGiven { get; set; } = new List<UserReview>();
        public ICollection<UserReview> ReviewsReceived { get; set; } = new List<UserReview>();
        public ICollection<LostItem> LostItems { get; set; } = new List<LostItem>();
    }
}
