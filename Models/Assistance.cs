using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WaslAlkhair.Api.Models;

public class Assistance
{
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required, MaxLength(255)]
	public string Title { get; set; } = string.Empty;

	[Required]
	public string Description { get; set; } = string.Empty;

	[Required]
	public int AvailableSpots { get; set; }

	[Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public DateTime? DescriptionUpdatedAt { get; set; } = DateTime.UtcNow; 

	[Required]
	public Guid AssistanceTypeId { get; set; }

	[ForeignKey(nameof(AssistanceTypeId))]
	public AssistanceType AssistanceType { get; set; } = null!;

	[Required]
	public string CreatedById { get; set; }

	[ForeignKey(nameof(CreatedById))]
	public AppUser CreatedBy { get; set; } = null!;

	[Required, MaxLength(255)]
	public string ContactInfo { get; set; } = string.Empty;

	public bool IsOpen { get; set; } = true;
}
