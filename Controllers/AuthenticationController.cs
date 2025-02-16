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
        private readonly EmailService _emailService;


        public AuthenticationController(IUserRepository userRepository, APIResponse response,
            IMapper mapper, UserManager<AppUser> userManager, EmailService emailService)
        {
            _userRepository = userRepository;
            _response = response;
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
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
                var user = _mapper.Map<AppUser>(request);
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
                // Temporarily comment out email confirmation logic
                /*
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailAsync(user.Email, "Confirm Your Email",
                    $"<h3>Verify Your Email</h3><p>Your confirmation code is: <strong>{code}</strong></p>");
                */

                return Ok(new { message = "Registration successful. Email verification is temporarily disabled." });
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        /*    [HttpPost("EmailVerification")]
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
            }*/

        /*[HttpGet("ValidateResetPasswordToken")]
          public async Task<IActionResult> ValidateResetPasswordToken(string email, string token)
          {
              var user = await _userManager.FindByEmailAsync(email);
              if (user == null)
              {
                  return BadRequest(new { message = "Invalid payload" });
              }

              // Validate the token
              var isValidToken = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);
              if (!isValidToken)
              {
                  return BadRequest(new { message = "Invalid or expired token." });
              }

              //TODO Redirect to a frontend page where the user can submit the new password
              // return Redirect($"https://yourfrontend.com/reset-password?email={email}&token={Uri.EscapeDataString(token)}");
          }*/

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
                var resetLink = $"https://localhost:5001/api/Authentication/ValidateResetPasswordToken?email={request.Email}&token={Uri.EscapeDataString(token)}";
                await _emailService.SendEmailAsync(user.Email, "Reset Your Password",
                    $"<h3>Reset Password</h3><p>Click the link below to reset your password:</p><a href='{resetLink}'>Reset Password</a>");
                return Ok(new { message = "A password reset link has been sent to your email. Please check your inbox." });

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
            var decodedToken = Uri.UnescapeDataString(request.Token);
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, decodedToken, request.Password);
            if (!resetPasswordResult.Succeeded)
            {
                return BadRequest(new { message = "Password reset failed.", errors = resetPasswordResult.Errors.Select(e => e.Description) });
            }
            return Ok(new { message = "Password reset successfully!" });
        }



        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> Login([FromBody] loginRequestDto request)
        {
            try
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

                // Temporarily comment out email confirmation check
                /*
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Email is not confirmed." };
                    return Unauthorized(_response);
                }
                */

                // Check if the password is correct for this email
                var isValidPassword = await _userRepository.CheckPasswordAsync(user, request.Password);
                if (!isValidPassword)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Incorrect Email or Password");
                    return BadRequest(_response);
                }

                // If Email and Password are correct, generate Token to return with User data
                var token = await _userRepository.CreateJwtToken(user);
                var role = await _userRepository.GetRoleAsync(user);  // Get the previously stored Role of the User
                object loginResponse = null;
                if (role == "User")   // If the Role is user, return UserDTO in result
                {
                    loginResponse = new LoginResponseDto<UserDTO>
                    {
                        User = _mapper.Map<UserDTO>(user),
                        Token = token,
                        Role = "User"
                    };
                }
                else if (role == "Charity")
                {
                    loginResponse = new LoginResponseDto<CharityDTO>
                    {
                        User = _mapper.Map<CharityDTO>(user),
                        Token = token,
                        Role = "Charity"
                    };
                }
                else
                {
                    loginResponse = new LoginResponseDto<AdminDTO>
                    {
                        User = _mapper.Map<AdminDTO>(user),
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
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }
    } 
}
