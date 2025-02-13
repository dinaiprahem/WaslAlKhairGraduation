
namespace WaslAlkhair.Api.DTOs.Authentication
{
    public class LoginResponseDto
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }

    }
}
