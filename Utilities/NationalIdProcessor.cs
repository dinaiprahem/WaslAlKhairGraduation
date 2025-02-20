using System;

namespace WaslAlkhair.Api.Utilities
{
	public static class NationalIdProcessor
	{
		public static DateOnly? ExtractDateOfBirth(string nationalId)
		{
			if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length != 14)
				return null;

			string dobPart = nationalId.Substring(1, 6);
			int year = int.Parse(dobPart.Substring(0, 2));
			int month = int.Parse(dobPart.Substring(2, 2));
			int day = int.Parse(dobPart.Substring(4, 2));

			int century = int.Parse(nationalId.Substring(0, 1));
			year += (century == 2) ? 1900 : 2000;

			return new DateOnly(year, month, day);
		}

		public static string ExtractGender(string nationalId)
		{
			if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length != 14)
				return "Unknown";

			return (int.Parse(nationalId.Substring(12, 1)) % 2 == 0) ? "Female" : "Male";
		}
	}
}
