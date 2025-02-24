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
        public DateOnly DateOfBirth { get; set; } // Creation Date for Charity , DateOfBirth for User
        public string? image {  get; set; }
        // Charity-specific properties
        public string? CharityRegistrationNumber { get; set; }
        public string? CharityMission { get; set; }

        public string? Address { get; set; }

    }
}
