using System.ComponentModel.DataAnnotations;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.Donation
{
    public class ResponseDonationOpportunityDetailsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string ImageUrl { get; set; }
        public decimal? RemainingAmount { get; set; }

        public decimal? CollectedAmount { get; set; } 

        public int NumberOfDonors { get; set; } 

        public int PageVisits { get; set; } 
    }
}
