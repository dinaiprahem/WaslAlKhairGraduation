using AutoMapper;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.DTOs.OpportunityParticipation;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Mappings
{
    public class OpportunityProfile : Profile
    {
        public OpportunityProfile() 
        {
            // Participation
            CreateMap<CreateOpportunityParticipation, OpportunityParticipation>();
            CreateMap<OpportunityParticipation , ResponsOpportunityParticipation>();
        }
    }
}
