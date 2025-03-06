using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Assistance
{
	public class AssistanceUpdateDTO
	{
		[Required]
		[MaxLength(255)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Description { get; set; } = string.Empty;

		[Required]
		public int AvailableSpots { get; set; }

		[Required]
		[MaxLength(255)]
		public string ContactInfo { get; set; } = string.Empty;

		public bool IsOpen { get; set; }
	}
}
