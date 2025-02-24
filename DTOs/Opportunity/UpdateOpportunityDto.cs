using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Opportunity
{
    public class UpdateOpportunityDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tasks are required")]
        public string Tasks { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Number of seats is required")]
        public int SeatsAvailable { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Benefits are required")]
        public string Benefits { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required age is required")]
        public int RequiredAge { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
    }
}