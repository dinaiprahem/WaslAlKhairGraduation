namespace WaslAlkhair.Api.DTOs.Assistance
{
	public class AssistanceWithCreatorDetailsDTO
	{
		public string TypeOfThisAssistance { get; set; } 
		public string CreatedByName { get; set; } 
		public string CreatedByProfilePic { get; set; }
		public string CreatedById { get; set; }
		public string Description { get; set; } 
		public int AvailableSpots { get; set; } 
		public DateTime CreatedAt { get; set; } 
		public string ContactInfo { get; set; } 
	}
}
