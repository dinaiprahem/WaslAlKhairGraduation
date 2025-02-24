using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaslAlkhair.Api.Models
{
	public class Opportunity
	{
		[Key]
		public int Id { get; set; }

		[Required, MaxLength(255)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Description { get; set; } = string.Empty;

		[Required]
		public string Tasks { get; set; } = string.Empty;

		[Required]
		public DateOnly StartDate { get; set; }

		[Required]
		public DateOnly EndDate { get; set; }

		[Required]
		public int SeatsAvailable { get; set; }

		[Required, MaxLength(255)]
		public string Location { get; set; } = string.Empty;

		[Required]
		public string Benefits { get; set; } = string.Empty;

		public bool IsClosed { get; set; } = false; // Default value

		[Required]
		public int RequiredAge { get; set; }

		[Required]
		public string Type { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? PhotoUrl { get; set; }  

		// Foreign Key - Creator (User or Charity)
		[Required]
		public string CreatedById { get; set; } = string.Empty;
		[ForeignKey(nameof(CreatedById))]
		public AppUser CreatedBy { get; set; } = null!;

		// Navigation property for participations
		public ICollection<OpportunityParticipation> Participants { get; set; } = new List<OpportunityParticipation>();

		// Auto-close opportunity when end date is reached
		public void CheckStatus()
		{
			IsClosed = DateOnly.FromDateTime(DateTime.UtcNow) >= EndDate;
		}
	}
}
