using AutoMapper;
using WaslAlkhair.Api.DTOs.Assistance;
using WaslAlkhair.Api.Models;

public class AssistanceProfile : Profile
{
	public AssistanceProfile()
	{
		// Mapping from AssistanceDTO to Assistance model (create operation)
		CreateMap<AssistanceDTO, Assistance>()
			.ForMember(dest => dest.AssistanceTypeId, opt => opt.MapFrom(src => src.AssistanceTypeId))
			.ForMember(dest => dest.AssistanceType, opt => opt.Ignore());  // Ignoring AssistanceType to avoid mapping issue

		// Mapping from Assistance model to AssistanceDetailsDTO 
		CreateMap<Assistance, AssistanceDetailsDTO>()
			.ForMember(dest => dest.AssistanceType, opt => opt.MapFrom(src => src.AssistanceType));

		// Mapping from Assistance model to AssistanceUpdateResponseDTO
		CreateMap<Assistance, AssistanceUpdateResponseDTO>()
			.ForMember(dest => dest.AssistanceType, opt => opt.MapFrom(src => src.AssistanceType))
			.ForMember(dest => dest.DaysSinceLastUpdate, opt => opt.MapFrom(src =>
				(src.DescriptionUpdatedAt.HasValue) ?
				(DateTime.UtcNow - src.DescriptionUpdatedAt.Value).Days : 0));

		// Mapping from AssistanceUpdateDTO to Assistance model (update operation)
		CreateMap<AssistanceUpdateDTO, Assistance>()
			.ForMember(dest => dest.AssistanceType, opt => opt.Ignore())
			.ForMember(dest => dest.AssistanceTypeId, opt => opt.Ignore())
			.ForMember(dest => dest.DescriptionUpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

		CreateMap<Assistance, AssistanceListDTO>()
	.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
	.ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
	.ForMember(dest => dest.CreatedByProfilePic, opt => opt.MapFrom(src => src.CreatedBy.image))
	.ForMember(dest => dest.DaysSinceLastUpdate, opt => opt.MapFrom(src =>
		(src.DescriptionUpdatedAt.HasValue) ?
		(DateTime.UtcNow - src.DescriptionUpdatedAt.Value).Days : 0));

		CreateMap<Assistance, AssistanceWithCreatorDetailsDTO>()
		.ForMember(dest => dest.TypeOfThisAssistance, opt => opt.MapFrom(src => src.AssistanceType.Name))
		.ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FullName))
		.ForMember(dest => dest.CreatedByProfilePic, opt => opt.MapFrom(src => src.CreatedBy.image)) 
		.ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedBy.Id))
		.ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
		.ForMember(dest => dest.AvailableSpots, opt => opt.MapFrom(src => src.AvailableSpots))
		.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
		.ForMember(dest => dest.ContactInfo, opt => opt.MapFrom(src => src.ContactInfo));

	}
}
