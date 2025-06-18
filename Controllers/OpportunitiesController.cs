using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs.Opportunity;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories;
using AutoMapper;
using Azure.Core;
using WaslAlkhair.Api.Services;
using Microsoft.AspNetCore.Identity;

[Route("api/[controller]")]
[ApiController]
public class OpportunitiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IOpportunityRepository _repository;
    private readonly APIResponse _response;
    private readonly IMapper _mapper;
    private readonly IFileService _fileStorageService;
    private readonly UserManager<AppUser> _userManager;

    public OpportunitiesController(AppDbContext context, IOpportunityRepository repository, IMapper mapper,
        IFileService fileStorageService, UserManager<AppUser> userManager)
    {
        _repository = repository;
        _context = context;
        _response = new APIResponse();
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
    }

    // Get All Opportunities
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> GetOpportunities()
    {
        try
        {
            var opportunities = await _context.Opportunities
                            .Include(o => o.CreatedBy)
                            .Where(o => o.IsClosed == false)
                            .ToListAsync();


            if (opportunities == null || !opportunities.Any())
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("No opportunities found.");
                return NotFound(_response);
            }

            var opportunityDtos = _mapper.Map<List<OpportunityDto>>(opportunities);

            foreach (var opportunityDto in opportunityDtos)
            {
                var user = opportunities
                    .FirstOrDefault(o => o.Id == opportunityDto.Id)?
                    .CreatedBy;

                if (user != null)
                {
                    string role;
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.FirstOrDefault() == "User")
                        role = "فرد";
                    else
                        role = "جمعيه";

                    opportunityDto.CreatedBy.Role = role;
                }
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = opportunityDtos;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add(ex.Message);
            return StatusCode(500, _response);
        }
    }

    [HttpGet("{id:int}", Name = "GetOpportunity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetOpportunity(int id)
    {
        try
        {
            var opportunity = await _context.Opportunities
                .Include(o => o.CreatedBy)
                .Include(o => o.Participants)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"Opportunity with ID {id} not found.");
                return NotFound(_response);
            }

            var opportunityDto = _mapper.Map<OpportunityDto>(opportunity);

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = opportunityDto;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add(ex.Message);
            return StatusCode(500, _response);
        }
    }

    [HttpPost("CreateOpportunity")]
   // [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> CreateOpportunity([FromForm] CreateOpportunityDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Validation failed. Please check the provided data.");
                _response.ErrorMessages.AddRange(ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(_response);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User ID not found in token. Please log in again.");
                return Unauthorized(_response);
            }

            var opportunity = _mapper.Map<Opportunity>(dto);
            opportunity.CreatedById = userId;
            opportunity.IsClosed = false;

            // Handle image upload
            if (dto.Image != null)
            {
                var imagePath = await _fileStorageService.UploadFileAsync(dto.Image);
                if (string.IsNullOrEmpty(imagePath))
                {
                    _response.StatusCode = HttpStatusCode.InternalServerError;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Failed to upload the image.");
                    return StatusCode(500, _response);
                }
                opportunity.PhotoUrl = imagePath;
            }

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<OpportunityDto>(opportunity);
            return CreatedAtRoute("GetOpportunity", new { id = opportunity.Id }, _response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("An error occurred while creating the opportunity.");
            _response.ErrorMessages.Add(ex.Message);
            return StatusCode(500, _response);
        }
    }

    [HttpDelete("{id:int}")]
    //[Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> DeleteOpportunity(int id)
    {
        try
        {
            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"Opportunity with ID {id} not found.");
                return NotFound(_response);
            }

            // Get the current user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

           // Check if the current user is the creator of the opportunity
            if (opportunity.CreatedById != userId)
            {
                _response.StatusCode = HttpStatusCode.Forbidden;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("You don't have permission to delete this opportunity.");
                return StatusCode(403, _response);
            }
         
            // Delete the associated image if it exists
            if (!string.IsNullOrEmpty(opportunity.PhotoUrl))
            {
                await _fileStorageService.DeleteFileAsync(opportunity.PhotoUrl);
            }

            _context.Opportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = "Opportunity deleted successfully.";
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add(ex.Message);
            return StatusCode(500, _response);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> UpdateOpportunity(int id, [FromForm] UpdateOpportunityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Validation failed. Please check the provided data.");
                _response.ErrorMessages.AddRange(ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(_response);
            }

            var existingOpportunity = await _context.Opportunities.FindAsync(id);
            if (existingOpportunity == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"Opportunity with ID {id} not found.");
                return NotFound(_response);
            }

            // Get the current user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the current user is the creator of the opportunity
            if (existingOpportunity.CreatedById != userId)
            {
                _response.StatusCode = HttpStatusCode.Forbidden;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("You don't have permission to update this opportunity.");
                return StatusCode(403, _response);
            }

            // Handle image update
            if (dto.Image != null)
            {
                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(existingOpportunity.PhotoUrl))
                {
                    await _fileStorageService.DeleteFileAsync(existingOpportunity.PhotoUrl);
                }

                // Upload the new image
                var imagePath = await _fileStorageService.UploadFileAsync(dto.Image);
                if (string.IsNullOrEmpty(imagePath))
                {
                    _response.StatusCode = HttpStatusCode.InternalServerError;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Failed to upload the new image.");
                    return StatusCode(500, _response);
                }
                existingOpportunity.PhotoUrl = imagePath;
            }

            // Map other fields from DTO to existing entity
            _mapper.Map(dto, existingOpportunity);

            _context.Opportunities.Update(existingOpportunity);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = "Opportunity updated successfully.";
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add(ex.Message);
            return StatusCode(500, _response);
        }
    }
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> SearchOpportunities([FromQuery] OpportunitySearchDto searchDto)
    {
        try
        {
            var searchParams = _mapper.Map<OpportunitySearchParams>(searchDto);

            var opportunities = await _repository.SearchOpportunitiesAsync(searchParams);

            if (opportunities == null || !opportunities.Any())
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("No opportunities found matching the search criteria.");
                return NotFound(_response);
            }

            var opportunityDtos = _mapper.Map<List<OpportunityDto>>(opportunities);

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = opportunityDtos;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add(ex.Message);
            return StatusCode(500, _response);
        }
    }
}