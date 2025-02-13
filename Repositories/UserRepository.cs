
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WaslAlkhair.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly JWTmodel _jwtOptions;

        public UserRepository(UserManager<AppUser> userManager , IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtOptions= configuration.GetSection("jwt").Get<JWTmodel>() ?? throw new ArgumentNullException(nameof(_jwtOptions), "JWT options cannot be null");
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUserAsync(AppUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> AddUserToRoleAsync(AppUser user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }
        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<string> CreateJwtToken(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(_jwtOptions.ExpirationInDays);

            var tokesDescriptor = new SecurityTokenDescriptor
            {
                Expires = expires,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = creds,
                Subject = new ClaimsIdentity(claims)
            };

            var securityToken= new JwtSecurityTokenHandler().CreateToken(tokesDescriptor);
            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        public async Task<string> GetRoleAsync(AppUser user)
        {
           var roles= await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }
    }
}
