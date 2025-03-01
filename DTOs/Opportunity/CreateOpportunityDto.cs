using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Opportunity
{
    public class CreateOpportunityDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tasks are required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Tasks must be between 5 and 500 characters")]
        public string Tasks { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [FutureDate(ErrorMessage = "Start date must be in the future")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [EndDateValidation("StartDate", ErrorMessage = "End date must be after start date")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Number of seats is required")]
        [Range(1, 1000, ErrorMessage = "Number of seats must be between 1 and 1000")]
        public int SeatsAvailable { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 255 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Benefits are required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Benefits must be between 5 and 500 characters")]
        public string Benefits { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required age is required")]
        [Range(16, 100, ErrorMessage = "Required age must be between 16 and 100")]
        public int RequiredAge { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Type must be between 2 and 50 characters")]
        public string Type { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }
    }

    // Custom validation attributes - add these in the same namespace
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateOnly date)
            {
                return date >= DateOnly.FromDateTime(DateTime.Today);
            }
            return false;
        }
    }

    public class EndDateValidationAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;

        public EndDateValidationAttribute(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not DateOnly endDate)
            {
                return new ValidationResult("Invalid end date format");
            }

            var propertyInfo = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            if (propertyInfo == null)
            {
                return new ValidationResult($"Unknown property: {_startDatePropertyName}");
            }

            var startDate = (DateOnly)propertyInfo.GetValue(validationContext.ObjectInstance);

            if (endDate <= startDate)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}