using WaslAlkhair.Api.DTOs.SMS;

namespace WaslAlkhair.Api.Services
{
    public interface ISMSService
    {
        Task<SMSResponseDto> SendAsync(SendSMSDto smsRequest);
        Task<SMSResponseDto> SendAsync(string mobileNumber, string body);
        Task<SMSResponseDto> SendGroupMMS(List<string> recipients, string text, List<string>? mediaUrls = null, string? subject = null);
        Task<List<SMSResponseDto>> GetReceivedMessagesAsync(DateTime? since = null);
    }
}
