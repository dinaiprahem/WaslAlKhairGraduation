
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.DTOs.Charity;
using WaslAlkhair.Api.DTOs.User;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;
using WaslAlkhair.Api.Services;

namespace WaslAlkhair.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IFileService fileService;
        private readonly JWTmodel _jwtOptions;

        public UserRepository(UserManager<AppUser> userManager , IConfiguration configuration 
            , IMapper mapper , IFileService fileService)
        {
            _userManager = userManager;
            _mapper = mapper;
            this.fileService = fileService;
            _jwtOptions = configuration.GetSection("jwt").Get<JWTmodel>() ?? throw new ArgumentNullException(nameof(_jwtOptions), "JWT options cannot be null");
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

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<AppUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<CharityDTO>> GetAllCharitesAsync()
        {
            var charities = await _userManager.GetUsersInRoleAsync("Charity");
            return _mapper.Map<List<CharityDTO>>(charities);
        }
        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var charities = await _userManager.GetUsersInRoleAsync("User");
            return _mapper.Map<List<UserDTO>>(charities);
        }

        public async Task<bool> UpdateCharityAsync(UpdateCharityDTO dto , AppUser charity)
        {

            charity.Email = dto.Email;
            charity.FullName = dto.CharityName;
            charity.CharityRegistrationNumber = dto.CharityRegistrationNumber;
            charity.CharityMission = dto.CharityMission;
            charity.DateOfBirth = (DateOnly)dto.EstablishedAt;
            charity.Address = dto.Address;
            charity.PhoneNumber = dto.PhoneNumber;

            // Handle image upload
            if (dto.Image != null)
            {
                var imagePath = await fileService.UploadFileAsync(dto.Image, "charity-logos");
                charity.image = imagePath;
            }

            var result = await _userManager.UpdateAsync(charity);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserAsync(updateUserDTO dto, AppUser user)
        {
            user.Email = dto.Email;
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-dto.Age));

            // Handle image upload
            if (dto.Image != null)
            {
                var imagePath = await fileService.UploadFileAsync(dto.Image, "user-profilePicture");
                user.image = imagePath;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }


    }
}

