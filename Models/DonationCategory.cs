using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.Models
{
    public class DonationCategory
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } // Category Name

        public string Description { get; set; } // Category Description

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public string ImageUrl { get; set; }

        // Navigation property للربط مع فرص التبرع
        public ICollection<DonationOpportunity> DonationOpportunities { get; set; }

        // Navigation property for Donations للربط مع التبرعات الي لكاتيجوري معين 
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
