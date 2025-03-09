using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.Models
{
    public class GiftDonation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RecipientName { get; set; }

        [Required]
        public string RecipientPhone { get; set; }

        public bool ShowAmount { get; set; } = false; // Whether to reveal the amount to the recipient
        public bool ShowOpportunity { get; set; } = false;

        // Foreign Key for the Donation (one-to-one)
        public int DonationId { get; set; }
        public Donation Donation { get; set; }
    }
}
