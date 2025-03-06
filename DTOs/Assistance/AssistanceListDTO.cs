public class AssistanceListDTO
{
	public Guid Id { get; set; } // ID of the assistance
	public string Title { get; set; } // Title of the assistance
	public string CreatedByProfilePic { get; set; } // Profile picture of the user
	public int DaysSinceLastUpdate { get; set; } // Days since last update of the assistance
}
