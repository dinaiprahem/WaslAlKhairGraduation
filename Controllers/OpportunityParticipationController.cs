
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WaslAlkhair.Api.DTOs.OpportunityParticipation;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpportunityParticipationController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;

        public OpportunityParticipationController(IUnitOfWork unitOfWork , IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
        }



        [Authorize(Roles = "User")]
        [HttpPost("/api/opportunities/{opportunityId}/participation")]
        [SwaggerOperation(Summary = "Apply for an opportunity by an authenticated user")]
        public async Task<IActionResult> Apply(int opportunityId, [FromBody] CreateOpportunityParticipation dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            if (dto.OpportunityId != opportunityId)
                return BadRequest("Opportunity ID in route and body do not match.");

            try
            {
                // ✅ Check if user already applied to this opportunity
                var existingParticipation = await _unitOfWork.OpportunityParticipation
                    .GetAsync(p => p.AppUserId == userId && p.OpportunityId == opportunityId);

                if (existingParticipation != null)
                    return Conflict("You have already applied for this opportunity.");

                // Map and fill fields
                var participation = mapper.Map<OpportunityParticipation>(dto);
                participation.AppUserId = userId;
                participation.ProcessNationalId(); // auto calculate Age, Gender

                await _unitOfWork.OpportunityParticipation.CreateAsync(participation);
                await _unitOfWork.SaveAsync();

                var toReturn = mapper.Map<ResponsOpportunityParticipation>(participation);
                return CreatedAtAction(nameof(GetParticipationDetails), new { id = participation.Id }, toReturn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "opportunity doesn't exist",
                    //details = ex.Message // remove in production
                });
            }
        }



        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get a Participation Details")]
        // [Authorize(Roles = "Charity,Admin")]
        public async Task<IActionResult> GetParticipationDetails(int id)
        {
            var participation = await _unitOfWork.OpportunityParticipation.GetAsync(o => o.Id == id);
            if (participation == null)
                return NotFound();

            var toReturn = mapper.Map<ResponsOpportunityParticipation>(participation);
            return Ok(toReturn);
        }



        [HttpGet("/api/opportunities/{opportunityId}/participations")]
        [SwaggerOperation(Summary = "Get list of participations for specific opportunity")]
        // [Authorize(Roles = "Charity")]
        public async Task<IActionResult> GetParticipationsByOpportunity(int opportunityId)
        {
            var participations = await _unitOfWork.OpportunityParticipation.GetAllAsync(p => p.OpportunityId == opportunityId);
            return Ok(participations);
        }



        [HttpGet("/api/charities/{charityId}/participations")]
        [SwaggerOperation(Summary = "Get All of participations for a Charity")]
        // [Authorize(Roles = "Charity")]
        public async Task<IActionResult> GetParticipationsByCharity(string charityId)
        {
            var participations = await _unitOfWork.OpportunityParticipation
                .GetAllAsync(p => p.Opportunity.CreatedById == charityId);
            return Ok(participations);
        }


        
    }
}
