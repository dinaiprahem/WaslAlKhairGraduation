using AutoMapper;

using WaslAlkhair.Api.DTOs.Opportunity;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Profiles
{
    public class OpportunityProfile : Profile
    {
        public OpportunityProfile()
        {
            // Mapping for nested properties
            CreateMap<AppUser, UserDto>();// Map AppUser to UserDto


            CreateMap<OpportunityParticipation, ParticipationDto>(); // Map OpportunityParticipation to ParticipationDto
            CreateMap<OpportunitySearchDto, OpportunitySearchParams>();

            // Mapping from Opportunity to OpportunityDto
            CreateMap<Opportunity, OpportunityDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy)) // Map CreatedBy
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants)); // Map Participants

            // Mapping from CreateOpportunityDto to Opportunity
            CreateMap<CreateOpportunityDto, Opportunity>();

            // Mapping from UpdateOpportunityDto to Opportunity
            CreateMap<UpdateOpportunityDto, Opportunity>();
        }
    }
}
