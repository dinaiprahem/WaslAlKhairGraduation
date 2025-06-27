using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ITokenBlacklist _tokenBlacklist;


        public AuthenticationController(IUserRepository userRepository, APIResponse response,
            IMapper mapper, UserManager<AppUser> userManager, EmailService emailService, ITokenBlacklist tokenBlacklist)
        {
            _userRepository = userRepository;
            _response = response;
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
            _tokenBlacklist = tokenBlacklist;
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
                    _response.ErrorMessages.Add("User is already exist.");
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
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Registerd Successfuly! Please Login";
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


        [HttpPost("Login")]
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

                // Check if the password is correct for this email
                var isValidPassword = await _userRepository.CheckPasswordAsync(user, request.Password);
                if (!isValidPassword)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Incorrect Email or Password");
                    return BadRequest(_response);
                }

                // Generate a new token
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

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(RequestForgotPasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid payload", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                // Validate user
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "No user found with the provided email address." });
                }

                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Failed to generate password reset token." });
                }

                var resetLink = $"http://localhost:5173/ResetPassword?email={request.Email}&token={Uri.EscapeDataString(token)}";

                await _emailService.SendEmailAsync(user.Email, "Reset Your Password - Wasl Al-Khair",
 $@"
<div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px;'>
    <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 8px rgba(0,0,0,0.05);'>
        <div style='background-color: #007bff; padding: 20px; text-align: center;'>
            <h1 style='color: white; margin: 0;'>Wasl Al-Khair</h1>
            <p style='color: #d1ecf1; margin-top: 8px; font-size: 14px;'>Connecting Goodness – وصل الخير</p>
        </div>

        <div style='padding: 30px;'>
            <h2 style='color: #333;'>Reset Your Password</h2>
            <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                We received a request to reset the password associated with this email address. If this was you, you can reset your password by clicking the button below:
            </p>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}' style='background-color: #007bff; color: white; padding: 14px 24px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: bold; display: inline-block;'>
                    Reset Password
                </a>
            </div>

            <p style='font-size: 14px; color: #777;'>
                If you didn't request a password reset, you can safely ignore this email. No changes will be made to your account.
            </p>
        </div>

        <div style='background-color: #f1f1f1; text-align: center; padding: 20px; font-size: 12px; color: #999;'>
            &copy; 2025 Wasl Al-Khair. All rights reserved.<br/>
            Empowering charity through technology.
        </div>
    </div>
</div>
");

                return Ok(new { message = "A password reset link has been sent to your email. Please check your inbox." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while processing the password reset request.", 
                    error = ex.Message 
                });
            }
        }


        [HttpGet("ValidateResetPasswordToken")]
        public async Task<IActionResult> ValidateResetPasswordToken(string email, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Email and token are required." });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest(new { message = "User not found with the provided email address." });
                }

                // Decode the token
                var decodedToken = Uri.UnescapeDataString(token);

                // Validate the token
                var isValidToken = await _userManager.VerifyUserTokenAsync(user, 
                    _userManager.Options.Tokens.PasswordResetTokenProvider, 
                    "ResetPassword", 
                    decodedToken);
                
                if (!isValidToken)
                {
                    return BadRequest(new { message = "Invalid or expired token." });
                }

                return Ok(new { message = "Token is valid.", isValid = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while validating the token.", 
                    error = ex.Message 
                });
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid payload", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }
                
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "User not found with the provided email address." });
                }
                
                // Decode the token properly
                var decodedToken = Uri.UnescapeDataString(request.Token);
                
                // Validate the token before attempting to reset password
                var isValidToken = await _userManager.VerifyUserTokenAsync(user, 
                    _userManager.Options.Tokens.PasswordResetTokenProvider, 
                    "ResetPassword", 
                    decodedToken);
                
                if (!isValidToken)
                {
                    return BadRequest(new { message = "Invalid or expired password reset token. Please request a new password reset." });
                }
                
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, decodedToken, request.Password);
                if (!resetPasswordResult.Succeeded)
                {
                    return BadRequest(new { 
                        message = "Password reset failed.", 
                        errors = resetPasswordResult.Errors.Select(e => e.Description) 
                    });
                }
                
                return Ok(new { message = "Password reset successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                return StatusCode(500, new { 
                    message = "An error occurred during password reset.", 
                    error = ex.Message 
                });
            }
        }

        [HttpPost("ChangePassword")]
        [Authorize] // Requires authentication
        public async Task<ActionResult<APIResponse>> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(_response);
                }

                // Validate that new password and confirm password match
                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("New password and confirm password do not match.");
                    return BadRequest(_response);
                }

                // Get the current user from the JWT token
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("User not authenticated.");
                    return Unauthorized(_response);
                }

                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("User not found.");
                    return NotFound(_response);
                }

                // Verify current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Current password is incorrect.");
                    return BadRequest(_response);
                }

                // Change the password
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = changePasswordResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Message = "Password changed successfully!";
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

        [HttpPost("SimpleChangePassword")]
        [Authorize] // Requires authentication
        public async Task<ActionResult<APIResponse>> SimpleChangePassword([FromBody] SimpleChangePasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(_response);
                }

                // Validate that new password and confirm password match
                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("New password and confirm password do not match.");
                    return BadRequest(_response);
                }

                // Get the current user from the JWT token
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("User not authenticated.");
                    return Unauthorized(_response);
                }

                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("User not found.");
                    return NotFound(_response);
                }

                // Generate a password reset token and use it to change the password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var changePasswordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                
                if (!changePasswordResult.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = changePasswordResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Message = "Password changed successfully!";
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

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto externalAuth)
        {
            var payload = await _emailService.VerifyGoogleToken(externalAuth);
            if (payload == null)
                return BadRequest(new { message = "Invalid Google token" });

            var user = await _userRepository.GetUserByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new AppUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    FullName = payload.Name
                };
                var createResult = await _userRepository.CreateUserAsync(user, null);
                if (!createResult.Succeeded)
                    return BadRequest(createResult.Errors);
            }

            var token = await _userRepository.CreateJwtToken(user);
            return Ok(new { token });
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

    }
}
