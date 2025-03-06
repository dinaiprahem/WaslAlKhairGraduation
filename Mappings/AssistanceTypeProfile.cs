using AutoMapper;
using WaslAlkhair.Api.DTOs.Assistance;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.MappingProfiles
{
	public class AssistanceTypeProfile : Profile
	{
		public AssistanceTypeProfile()
		{
			// Mapping from AssistanceType to AssistanceTypeDTO
			CreateMap<AssistanceType, AssistanceTypeDTO>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

			// Mapping from AssistanceTypeDTO to AssistanceType (for Create/Update scenarios)
			CreateMap<AssistanceTypeDTO, AssistanceType>()
				.ForMember(dest => dest.Id, opt => opt.Ignore()) // You might not want to set Id on create
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
		}
	}
}
