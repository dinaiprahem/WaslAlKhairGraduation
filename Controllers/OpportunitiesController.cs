using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Security.Claims;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs.Opportunity;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories;

[Route("api/[controller]")]
[ApiController]
public class OpportunitiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IOpportunityRepository _repository;

    public OpportunitiesController(AppDbContext context, IOpportunityRepository repository)
    {
        _repository = repository;
        _context = context;
    }

    // Get All Opportunities
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpportunities()
    {
        var opportunities = await _context.Opportunities
            .Include(o => o.CreatedBy) // Include the user who created the opportunity
            .ToListAsync();

        var opportunityDtos = opportunities.Select(o => new OpportunityDto
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            Tasks = o.Tasks,
            StartDate = o.StartDate,
            EndDate = o.EndDate,
            SeatsAvailable = o.SeatsAvailable,
            Location = o.Location,
            Benefits = o.Benefits,
            IsClosed = o.IsClosed,
            RequiredAge = o.RequiredAge,
            Type = o.Type,
            PhotoUrl = o.PhotoUrl,
            CreatedBy = new UserDto
            {
                Id = o.CreatedBy.Id,
                FullName = o.CreatedBy.FullName
            }
        }).ToList();

        return Ok(opportunityDtos);
    }

    [HttpGet("{id:int}", Name = "GetOpportunity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOpportunity(int id)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.CreatedBy)
            .Include(o => o.Participants)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
            return NotFound();

        var opportunityDto = new OpportunityDto
        {
            Id = opportunity.Id,
            Title = opportunity.Title,
            Description = opportunity.Description,
            Tasks = opportunity.Tasks,
            StartDate = opportunity.StartDate,
            EndDate = opportunity.EndDate,
            SeatsAvailable = opportunity.SeatsAvailable,
            Location = opportunity.Location,
            Benefits = opportunity.Benefits,
            IsClosed = opportunity.IsClosed,
            RequiredAge = opportunity.RequiredAge,
            Type = opportunity.Type,
            PhotoUrl = opportunity.PhotoUrl,
            CreatedBy = new UserDto
            {
                Id = opportunity.CreatedBy.Id,
                FullName = opportunity.CreatedBy.FullName
            },
            Participants = opportunity.Participants.Select(p => new ParticipationDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Age = p.Age,
                Gender = p.Gender,
                Email = p.Email,
                Specialization = p.Specialization,
                PhoneNumber = p.PhoneNumber,
                Address = p.Address
            }).ToList()
        };

        return Ok(opportunityDto);
    }

    [HttpPost("CreateOpportunity")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOpportunity([FromBody] CreateOpportunityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Validation failed. Please check the provided data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // Extract logged-in user ID from JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User ID not found in token. Please log in again." });
            }

            // Create a new opportunity instance
            var opportunity = new Opportunity
            {
                Title = dto.Title,
                Description = dto.Description,
                Tasks = dto.Tasks,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                SeatsAvailable = dto.SeatsAvailable,
                Location = dto.Location,
                Benefits = dto.Benefits,
                RequiredAge = dto.RequiredAge,
                Type = dto.Type,
                //PhotoUrl = dto.PhotoUrl,
                CreatedById = userId,
                IsClosed = false
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOpportunity), new { id = opportunity.Id }, opportunity);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "An error occurred while creating the opportunity.",
                Details = ex.Message
            });
        }
    }
    [HttpDelete("{id:int}")]
    [Authorize] // Ensure the user is authenticated
    [ProducesResponseType(StatusCodes.Status200OK)] // Changed to 200 OK for success message
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteOpportunity(int id)
    {
        var opportunity = await _context.Opportunities.FindAsync(id);
        if (opportunity == null)
        {
            return NotFound(new
            {
                Message = "Opportunity not found.",
                Id = id
            });
        }

        // Get the current user's ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Check if the current user is the creator of the opportunity
        if (opportunity.CreatedById != userId)
        {
            return Forbid(); // Return 403 Forbidden if the user is not the creator
        }

        _context.Opportunities.Remove(opportunity);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Opportunity deleted successfully."
        });
    }
    [HttpPut("{id:int}")]
    [Authorize] // Ensure the user is authenticated
    [ProducesResponseType(StatusCodes.Status200OK)] // Changed to 200 OK for success message
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOpportunity(int id, [FromBody] UpdateOpportunityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                Message = "Validation failed. Please check the provided data.",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }

        var existingOpportunity = await _context.Opportunities.FindAsync(id);
        if (existingOpportunity == null)
        {
            return NotFound(new
            {
                Message = "Opportunity not found.",
                Id = id
            });
        }

        // Get the current user's ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Check if the current user is the creator of the opportunity
        if (existingOpportunity.CreatedById != userId)
        {
            return Forbid(); // Return 403 Forbidden if the user is not the creator
        }

        // Update the existing opportunity with the new data
        existingOpportunity.Title = dto.Title;
        existingOpportunity.Description = dto.Description;
        existingOpportunity.Tasks = dto.Tasks;
        existingOpportunity.StartDate = dto.StartDate;
        existingOpportunity.EndDate = dto.EndDate;
        existingOpportunity.SeatsAvailable = dto.SeatsAvailable;
        existingOpportunity.Location = dto.Location;
        existingOpportunity.Benefits = dto.Benefits;
        existingOpportunity.RequiredAge = dto.RequiredAge;
        existingOpportunity.Type = dto.Type;
        existingOpportunity.PhotoUrl = dto.PhotoUrl;

        _context.Opportunities.Update(existingOpportunity);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Opportunity updated successfully."
        });
    }

    [HttpPatch("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PatchOpportunity(int id, JsonPatchDocument<UpdateOpportunityDto> patchDTO)
    {
        if (patchDTO == null)
        {
            return BadRequest(new
            {
                Message = "Patch document is required."
            });
        }

        var existingOpportunity = await _repository.GetOpportunityByIdAsync(id);
        if (existingOpportunity == null)
        {
            return NotFound(new
            {
                Message = "Opportunity not found.",
                Id = id
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (existingOpportunity.CreatedById != userId)
        {
            return Forbid();
        }

        // Create a DTO to apply the patch
        var opportunityToPatch = new UpdateOpportunityDto
        {
            Title = existingOpportunity.Title,
            Description = existingOpportunity.Description,
            Tasks = existingOpportunity.Tasks,
            StartDate = existingOpportunity.StartDate,
            EndDate = existingOpportunity.EndDate,
            SeatsAvailable = existingOpportunity.SeatsAvailable,
            Location = existingOpportunity.Location,
            Benefits = existingOpportunity.Benefits,
            RequiredAge = existingOpportunity.RequiredAge,
            Type = existingOpportunity.Type,
            PhotoUrl = existingOpportunity.PhotoUrl
        };

        // Apply the patch
        patchDTO.ApplyTo(opportunityToPatch);

        // Validate the patched model
        if (!TryValidateModel(opportunityToPatch))
        {
            return BadRequest(new
            {
                Message = "Invalid patch document.",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }

        // Map the patched DTO back to the entity
        existingOpportunity.Title = opportunityToPatch.Title;
        existingOpportunity.Description = opportunityToPatch.Description;
        existingOpportunity.Tasks = opportunityToPatch.Tasks;
        existingOpportunity.StartDate = opportunityToPatch.StartDate;
        existingOpportunity.EndDate = opportunityToPatch.EndDate;
        existingOpportunity.SeatsAvailable = opportunityToPatch.SeatsAvailable;
        existingOpportunity.Location = opportunityToPatch.Location;
        existingOpportunity.Benefits = opportunityToPatch.Benefits;
        existingOpportunity.RequiredAge = opportunityToPatch.RequiredAge;
        existingOpportunity.Type = opportunityToPatch.Type;
        existingOpportunity.PhotoUrl = opportunityToPatch.PhotoUrl;

        await _repository.UpdateOpportunityAsync(existingOpportunity);
        return Ok(new
        {
            Message = "Opportunity patched successfully."
        });
    }
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchOpportunities([FromQuery] OpportunitySearchDto searchDto)
    {
        var searchParams = new OpportunitySearchParams
        {
            Location = searchDto.Location,
            Type = searchDto.Type,
            StartDate = searchDto.StartDate,
            EndDate = searchDto.EndDate,
            MinAge = searchDto.MinAge,
            MaxSeats = searchDto.MaxSeats,
            IsOpen = searchDto.IsOpen,
            SearchTerm = searchDto.SearchTerm
        };

        var opportunities = await _repository.SearchOpportunitiesAsync(searchParams);

        var opportunityDtos = opportunities.Select(o => new OpportunityDto
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            Tasks = o.Tasks,
            StartDate = o.StartDate,
            EndDate = o.EndDate,
            SeatsAvailable = o.SeatsAvailable,
            Location = o.Location,
            Benefits = o.Benefits,
            IsClosed = o.IsClosed,
            RequiredAge = o.RequiredAge,
            Type = o.Type,
            PhotoUrl = o.PhotoUrl,
            CreatedBy = new UserDto
            {
                Id = o.CreatedBy.Id,
                FullName = o.CreatedBy.FullName
            }
        }).ToList();

        return Ok(opportunityDtos);
    }

}