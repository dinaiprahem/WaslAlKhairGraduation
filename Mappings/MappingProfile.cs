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

            // Category
            CreateMap<DonationCategory,ResponseDonationCategoryDTO >()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));

            // Opportunity 
            CreateMap<CreateDonationOpportunityDTO, DonationOpportunity>()
                  .ForMember(dest => dest.Category, opt => opt.Ignore());

            CreateMap<UpdateDonationOpportunityDTO, DonationOpportunity>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // سيتم تحديثها يدويًا بعد رفع الصورة
                .ForMember(dest => dest.Status, opt => opt.Ignore())   // سيتم تحديدها منطقيًا
                .ForMember(dest => dest.CollectedAmount, opt => opt.Ignore()) // لا يتم التعديل عليها هنا
                .ForMember(dest => dest.PageVisits, opt => opt.Ignore()) // مش محتاجينها هنا
                .ForMember(dest => dest.NumberOfDonors, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Charity, opt => opt.Ignore());

            CreateMap<DonationOpportunity, ResponseDonationOpportunityDetailsDTO>()
                .ForMember(dest => dest.RemainingAmount, opt =>
                    opt.MapFrom(src => src.TargetAmount.HasValue
                        ? src.TargetAmount - src.CollectedAmount
                        : (decimal?)null));

            CreateMap<DonationOpportunity, ResponseAllDonationOpportunities>();
        }
    }
}
