namespace WaslAlkhair.Api.Helpers
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromPhoneNumber { get; set; } = string.Empty;
        public string? WebhookUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
}
