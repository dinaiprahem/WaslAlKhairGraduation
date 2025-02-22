using System;

namespace WaslAlkhair.Api.Utilities
{
	public static class AgeCalculator
	{
		public static int CalculateAge(DateOnly? dateOfBirth)
		{
			if (!dateOfBirth.HasValue)
				return 0;

			var today = DateOnly.FromDateTime(DateTime.UtcNow); // Convert today's date to DateOnly
			var birthdate = dateOfBirth.Value;
			var age = today.Year - birthdate.Year;

			// Adjust age if the birthday hasn't occurred yet this year
			if (birthdate > today.AddYears(-age))
				age--;

			return age;
		}
	}
}
