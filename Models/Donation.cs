using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.Models
{

    public class Donation
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DonatedAt { get; set; } = DateTime.UtcNow;

        // Donor details
        public string? DonorId { get; set; }
        public AppUser? Donor { get; set; }



        //************* For Diffrent Donation Types ************//
     
        public DonationType Type { get; set; }
     

        // For Category donations
        public int? CategoryId { get; set; }
        public DonationCategory? Category { get; set; }
      

        // For tracking distributions 
        public ICollection<DonationDistribution> Distributions { get; set; } = new List<DonationDistribution>();

        // For Gifted donations
        //public GiftDonation? GiftDonation { get; set; }

   
    }

    public enum DonationType
    {
        Opportunity, // Direct donation to a specific opportunity
        Category,    // Donation split across a category
        Fast,        // Donation split randomly
        Gifted       // Gifted donation to a specific opportunity
    }
}
