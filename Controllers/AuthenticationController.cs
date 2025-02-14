using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly APIResponse _response;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public AuthenticationController(IUserRepository userRepository, APIResponse response,
            IMapper mapper, UserManager<AppUser> userManager)
        {
            _userRepository = userRepository;
            _response = response;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult<APIResponse>> Register([FromBody] RegisterRequestDto request)
        {

            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Email is already taken.");
                    return BadRequest(_response);
                }

                // Create user
                //var user = _mapper.Map<AppUser>(request);


                var user = new AppUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Major = request.Major,
                    DateOfBirth = request.DateOfBirth
                };

                var createResult = await _userRepository.CreateUserAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = createResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }

                // Add user to role
                var roleResult = await _userRepository.AddUserToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = roleResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }
                // Require email confirmation before signing in
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // Email functionality to send the code to the user
                return Ok(new { message = $"Please confirm your email with the code that you have received! Here's the code: {code}" });

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPost("EmailVerification")]
        public async Task<IActionResult> EmailVerification(string? email, string? code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "Invalid payload" });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid payload" });
            }

            var isVerified = await _userManager.ConfirmEmailAsync(user, code);
            if (!isVerified.Succeeded)
            {
                return BadRequest(new { message = "Email confirmation failed.", errors = isVerified.Errors.Select(e => e.Description) });
            }

            return Ok(new { message = "Email confirmed successfully!" });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(RequestForgotPasswordDto request)
        {
            if (ModelState.IsValid)
            {
                //Valudate user
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Invalid payload" });
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Something went wrong" });
                }
                var callbackUrl = $"https://localhost:5001/api/Authentication/ResetPassword?email={request.Email}&token={token}";
                // Email functionality to send the code to the user
                return Ok(new
                {
                    token = token,
                    email = user.Email,
                });

            }
            return BadRequest(new { message = "Invalid payload" });
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid payload" });
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid payload" });
            }
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (!resetPasswordResult.Succeeded)
            {
                return BadRequest(new { message = "Password reset failed.", errors = resetPasswordResult.Errors.Select(e => e.Description) });
            }
            return Ok(new { message = "Password reset successfully!" });
        }


        [HttpPost("login")]
        public async Task<ActionResult<APIResponse>> Login([FromBody] loginRequestDto request)
        {
            // Check if the email is registered before
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { "User not found." };
                return Unauthorized(_response);
            }

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { "Email is not confirmed." };
                return Unauthorized(_response);
            }
            // Check if the password is correct for this email
            var isValidPassword = await _userRepository.CheckPasswordAsync(user, request.Password);
            if (!isValidPassword)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Incorrect Email or Password");
                return BadRequest(_response);
            }

            //If Email and Password are correct , Generate Token to return with User data
            var token = await _userRepository.CreateJwtToken(user);
            var role = await _userRepository.GetRoleAsync(user);  //Get the previously stored Role of the User
            object loginResponse = null;
            if (role == "User")   //If the Role is user , return UserDTO in result
            {
                var userToReturn = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Major = user.Major,
                    Age = "15"
                };
                loginResponse = new LoginResponseDto<UserDTO>
                {
                    User = userToReturn,
                    Token = token,
                    Role = "User"
                };
            }
            else if (role == "Charity")
            {
                var userToReturn = new CharityDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    CharityName = user.FullName,
                    CharityMission = user.CharityMission,
                    CharityRegistrationNumber = user.CharityRegistrationNumber
                };
                loginResponse = new LoginResponseDto<CharityDTO>
                {
                    User = userToReturn,
                    Token = token,
                    Role = "Charity"
                };
            }
            else
            {
                var userToReturn = new AdminDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                };
                loginResponse = new LoginResponseDto<AdminDTO>
                {
                    User = userToReturn,
                    Token = token,
                    Role = "Admin"
                };
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Message = "Login successful";
            _response.Result = loginResponse;
            return Ok(_response);
        }
    }
}
