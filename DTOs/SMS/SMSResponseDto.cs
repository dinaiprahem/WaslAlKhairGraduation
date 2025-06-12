namespace WaslAlkhair.Api.DTOs.SMS
{
    public class SMSResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }
        public DateTime SentAt { get; set; }
        public string? To { get; set; }
        public string? From { get; set; }
        public string? MessageType { get; set; } // SMS or MMS
        public Dictionary<string, object>? ProviderData { get; set; }
    }
}
