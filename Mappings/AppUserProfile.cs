using AutoMapper;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.Utilities; 

namespace WaslAlkhair.Api.Profiles
{
	public class AppUserProfile : Profile
	{
		public AppUserProfile()
		{
			/////----- Register ------/////
			// RegisterRequestDto -> AppUser
			CreateMap<RegisterRequestDto, AppUser>()
				.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Map Email to UserName
				.ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
					src.Age.HasValue
						? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-src.Age.Value))
						: (DateOnly?)null))
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow)); // Set CreatedAt to current UTC time

			/////----- Login ------/////
			// AppUser -> UserDTO
			CreateMap<AppUser, UserDTO>()
				.ForMember(dest => dest.Age, opt => opt.MapFrom(src => AgeCalculator.CalculateAge(src.DateOfBirth)));

			// AppUser -> CharityDTO
			CreateMap<AppUser, CharityDTO>()
				.ForMember(dest => dest.CharityName, opt => opt.MapFrom(src => src.FullName))
				.ForMember(dest => dest.EstablishedAt, opt => opt.MapFrom(src => src.DateOfBirth));

			// AppUser -> AdminDTO
			CreateMap<AppUser, AdminDTO>();
		}
	}
}
