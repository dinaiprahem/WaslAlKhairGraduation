using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.Models
{
	public class AssistanceType
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required, MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		// one to many as one assistant type has many assistance
		public ICollection<Assistance> Assistances { get; set; } = new List<Assistance>();
	}
}
