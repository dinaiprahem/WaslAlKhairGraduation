using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Hosting;

namespace WaslAlkhair.Api.Models
{
    public class DonationOpportunity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } 

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }
        public decimal? TargetAmount { get; set; } 

        public decimal CollectedAmount { get; set; } = 0;

        public int NumberOfDonors { get; set; } = 0;
        public DateTime? Deadline { get; set; }
        public OpportunityStatus Status { get; set; } = OpportunityStatus.Active;

        public int PageVisits { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // ******************* Relations ********************** //

        // Opportunity Category 
        [Required]
        public int CategoryId { get; set; }
        public DonationCategory Category { get; set; }

        // Charity Created the opportunity
        public string CharityId { get; set; }
        public AppUser Charity { get; set; }


        // Navigation property للربط مع التبرعات
        public ICollection<DonationDistribution> DonationDistribution { get; set; } = new List<DonationDistribution>();

    }

    public enum OpportunityStatus
    {
        Active,
        Completed,
        Closed
    }

}
