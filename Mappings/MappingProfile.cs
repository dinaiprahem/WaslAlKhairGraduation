using AutoMapper;
using WaslAlkhair.Api.DTOs;
using WaslAlkhair.Api.Models;
using System.Linq;
using WaslAlkhair.Api.DTOs.Donation;

namespace WaslAlkhair.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map from Donation to DonationDto
            CreateMap<Donation, DonationDto>()
                .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src => src.Donor != null ? src.Donor.UserName : "Anonymous"))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.Distributions, opt => opt.MapFrom(src => src.Distributions));

            // Map from DonationDistribution to DistributionDto
            CreateMap<DonationDistribution, DistributionDto>()
                .ForMember(dest => dest.OpportunityId, opt => opt.MapFrom(src => src.DonationOpportunityId))
                .ForMember(dest => dest.OpportunityTitle, opt => opt.MapFrom(src => src.DonationOpportunity.Title))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.DistributedAmount));

            // Map between GiftDonation and GiftDonationDto (both ways)
            CreateMap<GiftDonation, GiftDonationDto>().ReverseMap();
        }
    }
}
