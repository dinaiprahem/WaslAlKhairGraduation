using AutoMapper;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.DTOs.Authentication;

namespace WaslAlkhair.Api.Profiles
{
    public class AppUserProfile : Profile
    {
        public AppUserProfile()
        {
            /////----- Register ------////
            // RegisterRequestDto ->  AppUser
            CreateMap<RegisterRequestDto, AppUser>()
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Map Email to UserName
                  .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
                          src.Age.HasValue
                         ? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-src.Age.Value))
                         : (DateOnly?)null)) // Convert to DateOnly
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow)); // Set CreatedAt to current UTC time


            /////----- Login ------////
            //  AppUser -> UserDTO
            CreateMap<AppUser, UserDTO>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateOfBirth)));

            //  AppUser -> CharityDTO
            CreateMap<AppUser, CharityDTO>()
                 .ForMember(dest => dest.CharityName, opt => opt.MapFrom(src => src.FullName));

            //  AppUser -> AdminDTO
            CreateMap<AppUser, AdminDTO>();
        }



        // Helper method to calculate age from DateOfBirth
        private int CalculateAge(DateOnly? dateOfBirth)
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