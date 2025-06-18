using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.SMS
{
    public class SendSMSDto
    {
        [Required(ErrorMessage = "At least one recipient is required")]
        public List<string> To { get; set; } = new();

        [Required(ErrorMessage = "Message text is required")]
        [StringLength(1600, ErrorMessage = "Message text cannot exceed 1600 characters")]
        public string Text { get; set; } = string.Empty;

        public string? Subject { get; set; }

        public List<string>? MediaUrls { get; set; }

        public string MessageType { get; set; } = "SMS"; // SMS or MMS

        public string? CustomFromNumber { get; set; }

        public string? WebhookUrl { get; set; }
    }
}
