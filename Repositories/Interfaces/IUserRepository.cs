using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(AppUser user, string password);
        Task<IdentityResult> AddUserToRoleAsync(AppUser user, string role);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task<String> GetRoleAsync(AppUser user);
        Task<string> CreateJwtToken(AppUser user);
    }
}
