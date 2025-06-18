using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.SMS
{
    public class GroupMMSDto
    {
        [Required(ErrorMessage = "At least one recipient is required")]
        [MinLength(1, ErrorMessage = "At least one recipient is required")]
        public List<string> Recipients { get; set; } = new();

        [Required(ErrorMessage = "Message text is required")]
        [StringLength(1600, ErrorMessage = "Message text cannot exceed 1600 characters")]
        public string Text { get; set; } = string.Empty;

        public string? Subject { get; set; }

        public List<string>? MediaUrls { get; set; }
    }
}
