using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.DTOs.Charity;
using WaslAlkhair.Api.DTOs.User;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {      
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task<string> CreateJwtToken(AppUser user);

        // Create
        Task<IdentityResult> CreateUserAsync(AppUser user, string password);
        Task<IdentityResult> AddUserToRoleAsync(AppUser user, string role);

        // Get
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<String> GetRoleAsync(AppUser user);
        Task<AppUser?> GetUserByIdAsync(string userId);
        Task<List<CharityDTO>> GetAllCharitesAsync();
        Task<List<UserDTO>> GetAllUsersAsync();

        // Delete
        Task<bool> DeleteUserAsync(string userId);
        
        // Update 
        Task<bool> UpdateCharityAsync(UpdateCharityDTO dto , AppUser charity);
        Task<bool> UpdateUserAsync(updateUserDTO dto, AppUser user);


    }
}
