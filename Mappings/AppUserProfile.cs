using AutoMapper;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Mappings
{
    public class AppUserProfile : Profile
    {
        public AppUserProfile()
        {
            /*
            CreateMap<RegisterRequestDto, AppUser>()
            
             .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email) )// Map Email to UserName
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber)) // Map PhoneNumber
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
             .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
             .ForMember(dest => dest.Major, opt => opt.MapFrom(src => src.Major))
             .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth));
            */

        }
        
    }
}
