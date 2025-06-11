using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WaslAlkhair.Api.DTOs.Assistance;

namespace WaslAlkhair.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AssistanceController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AssistanceController(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateAssistance([FromBody] AssistanceDTO assistanceDto)
		{
			if (assistanceDto == null)
			{
				return BadRequest("Invalid data.");
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { Message = "User ID not found in token. Please log in again." });
			}

			var assistance = _mapper.Map<Assistance>(assistanceDto);

			assistance.CreatedById = userId;
			assistance.CreatedAt = DateTime.UtcNow;

			var assistanceType = await _unitOfWork.AssistanceTypeRepository.GetAsync(at => at.Id == assistanceDto.AssistanceTypeId);
			if (assistanceType == null)
			{
				return BadRequest("Assistance type not found.");
			}

			assistance.AssistanceType = assistanceType;

			await _unitOfWork.AssistanceRepository.CreateAsync(assistance);

			var result = await _unitOfWork.SaveAsync();
			if (result)
			{
				var createdAssistance = await _unitOfWork.AssistanceRepository.GetByIdAsync(assistance.Id);
				var createdAssistanceDto = _mapper.Map<AssistanceDetailsDTO>(createdAssistance);
				return CreatedAtAction(nameof(GetAssistanceById), new { id = assistance.Id }, createdAssistanceDto);
			}

			return BadRequest("Could not create assistance.");
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetAssistanceById(Guid id)
		{
			var assistance = await _unitOfWork.AssistanceRepository.GetByIdAsync(id);
			if (assistance == null)
			{
				return NotFound("Assistance not found.");
			}

			var assistanceDto = _mapper.Map<AssistanceDetailsDTO>(assistance);
			return Ok(assistanceDto);
		}

		//the user who create it or the admin both they can delete it 
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteAssistance(Guid id)
		{
		
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var userRole = User.FindFirst(ClaimTypes.Role)?.Value; 

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { Message = "User ID not found in token. Please log in again." });
			}

			var assistance = await _unitOfWork.AssistanceRepository.GetByIdAsync(id);
			if (assistance == null)
			{
				return NotFound(new { Message = "Assistance not found." });
			}
			//check if admin or user 
			if (assistance.CreatedById != userId && userRole != "Admin")
			{
				return Forbid();
			}

			_unitOfWork.AssistanceRepository.Delete(assistance);

			var result = await _unitOfWork.SaveAsync();
			if (result)
			{
				return Ok(new { Message = "Assistance deleted successfully." });
			}

			return BadRequest(new { Message = "Failed to delete assistance." });
		}

		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateAssistance(Guid id, [FromBody] AssistanceUpdateDTO updateDto)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { Message = "User ID not found in token. Please log in again." });
			}

			var assistance = await _unitOfWork.AssistanceRepository.GetByIdAsync(id);
			if (assistance == null)
			{
				return NotFound(new { Message = "Assistance not found." });
			}

			if (assistance.CreatedById != userId)
			{
				return Forbid();
			}

			_mapper.Map(updateDto, assistance);

			assistance.DescriptionUpdatedAt = DateTime.UtcNow;
			_unitOfWork.AssistanceRepository.Update(assistance);
			var result = await _unitOfWork.SaveAsync();
			if (result)
			{
				var updatedAssistanceDto = _mapper.Map<AssistanceUpdateResponseDTO>(assistance);
				return Ok(new { Message = "Assistance updated successfully.", Assistance = updatedAssistanceDto });
			}

			return BadRequest(new { Message = "Failed to update assistance." });
		}


		[HttpGet("assistance-types")]
		public async Task<IActionResult> GetAssistanceTypes()
		{
			var assistanceTypes = await _unitOfWork.AssistanceTypeRepository.GetAllAsync();

			if (assistanceTypes == null || !assistanceTypes.Any())
			{
				return NotFound(new { Message = "No assistance types found." });
			}

			var assistanceTypeDTOs = _mapper.Map<List<AssistanceTypeDTO>>(assistanceTypes);

			return Ok(assistanceTypeDTOs);
		}
		//get the open assistances for each type
		[HttpGet("SearchByAssistanceType/{assistanceTypeId}")]
		public async Task<IActionResult> GetAssistancesByType(Guid assistanceTypeId)
		{
			var assistances = await _unitOfWork.AssistanceRepository.GetAssistancesByTypeAsync(assistanceTypeId);


			if (assistances == null || !assistances.Any())
			{
				return NotFound(new { Message = "No assistances found for this type." });
			}

			var assistanceDtos = _mapper.Map<List<AssistanceListDTO>>(assistances);

			return Ok(new { Assistances = assistanceDtos });
		}

		//get the details information about assisstance
		[HttpGet("AssistanceWithCreatorDetails/{id}")]
		public async Task<IActionResult> GetAssistanceDetails(Guid id)
		{
			var assistance = await _unitOfWork.AssistanceRepository.GetByIdAsync(id);

			if (assistance == null)
			{
				return NotFound(new { Message = "Assistance not found." });
			}

			var assistanceDetailsDto = _mapper.Map<AssistanceWithCreatorDetailsDTO>(assistance);

			return Ok(new { Assistance = assistanceDetailsDto });
		}

		//get all assistances created by specific user 
		[HttpGet("GetAssistancesByUser/{userId}")]
		public async Task<IActionResult> GetAssistancesByUser(string userId)
		{
			var assistances = await _unitOfWork.AssistanceRepository.GetByUserIdAsync(userId);

			if (assistances == null || !assistances.Any())
			{
				return NotFound(new { Message = "No assistances found for this user." });
			}
			var assistanceDtos = _mapper.Map<List<AssistanceDetailsDTO>>(assistances);

			return Ok(new { Assistances = assistanceDtos });
		}
		


	}
}
