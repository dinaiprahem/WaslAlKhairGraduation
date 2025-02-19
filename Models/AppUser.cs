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


        // Charity-specific properties
        public string? CharityRegistrationNumber { get; set; }
        public string? CharityMission { get; set; }


        // Normal User-specific properties
        public DateTime DateOfBirth { get; set; }

    }
}
