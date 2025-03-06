namespace WaslAlkhair.Api.DTOs.Assistance
{
	public class AssistanceDetailsDTO
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public int AvailableSpots { get; set; }
		public DateTime CreatedAt { get; set; }
		public string ContactInfo { get; set; } = string.Empty;
		public bool IsOpen { get; set; }
		public AssistanceTypeDTO AssistanceType { get; set; } = new AssistanceTypeDTO();
		public string CreatedById { get; set; } = string.Empty;

	}
}
