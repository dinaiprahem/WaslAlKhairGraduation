using System;
using WaslAlkhair.Api.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaslAlkhair.Api.Models
{
	public class OpportunityParticipation
	{
		private string _nationalId;
		[Key]
		public int Id { get; set; }

		[Required]
		public string AppUserId { get; set; } 
		[ForeignKey("AppUserId")]
		public AppUser AppUser { get; set; }

		[Required]
		public int OpportunityId { get; set; }
		[ForeignKey("OpportunityId")]
		public Opportunity Opportunity { get; set; }

		[Required]
		public string FullName { get; set; }

		[Required]
		[MaxLength(14), MinLength(14)]
		public string NationalId
		{
			get => _nationalId;
			set
			{
				_nationalId = value;
				ProcessNationalId(); // Automatically update Age and Gender
			}
		}

		[Required]
		public int Age { get; private set; }

		[Required]
		public string Gender { get; private set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Specialization { get; set; }

		[Required]
		[Phone]
		public string PhoneNumber { get; set; }

		[Required]
		public string Address { get; set; }

		/// Extracts Age and Gender from National ID and stores them.
		public void ProcessNationalId()
		{
			var dateOfBirth = NationalIdProcessor.ExtractDateOfBirth(NationalId);
			Age = AgeCalculator.CalculateAge(dateOfBirth); // Store Age in DB
			Gender = NationalIdProcessor.ExtractGender(NationalId); // Store Gender in DB
		}
	}
}
