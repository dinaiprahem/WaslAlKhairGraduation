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

                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Registration successful. Please log in.";
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

        [HttpPost("login")]
        public async Task<ActionResult<APIResponse>> Login([FromBody] loginRequestDto request)
        {
            //check if the email is registerd before
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            //After we check Email , Check if the password is correct for this Email
            var isValidPassword = await _userRepository.CheckPasswordAsync(user, request.Password);

            //If one of them filed , Login Faild
            if (user == null || !isValidPassword)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Incorrect Email or Password");
                return BadRequest(_response);
            }

            //If Email and Password are correct , Generate Token to return with User data
            var token = await _userRepository.CreateJwtToken(user);
            var userToReturn = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Major = user.Major,
                Age = "15"
            };
            var loginResponse = new LoginResponseDto
            {
                User = userToReturn,
                Token = token,
                Role = "User"
            };
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Message = "Login successful";
            _response.Result = loginResponse;
            return Ok(_response);
        }
    }
}
