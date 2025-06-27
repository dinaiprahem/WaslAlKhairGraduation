namespace WaslAlkhair.Api.DTOs.Authentication
{
    public class SimpleChangePasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
} 