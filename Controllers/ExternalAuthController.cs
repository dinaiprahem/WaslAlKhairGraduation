using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Controllers
{
	[Route("api/external-auth")]
	[ApiController]
	public class ExternalAuthController : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IConfiguration _configuration;

		public ExternalAuthController(IUserRepository userRepository, IConfiguration configuration)
		{
			_userRepository = userRepository;
			_configuration = configuration;
		}

		[HttpPost("google-login")]
		public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto externalAuth)
		{
			var payload = await VerifyGoogleToken(externalAuth);
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

		private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(GoogleAuthDto externalAuth)
		{
			try
			{
				var settings = new GoogleJsonWebSignature.ValidationSettings
				{
					Audience = new List<string> { _configuration["Authentication:Google:ClientId"] }
				};
				return await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken, settings);
			}
			catch
			{
				return null;
			}
		}
	}
}
