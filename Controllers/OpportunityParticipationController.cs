
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



        [HttpPost("/api/opportunities/{opportunityId}/participation")]
        [SwaggerOperation(Summary = "Apply for an opportunity")]
        public async Task<IActionResult>Apply(CreateOpportunityParticipation dto)
        { 

            var participation = mapper.Map<OpportunityParticipation>(dto);

            await _unitOfWork.OpportunityParticipation.CreateAsync(participation);
            await _unitOfWork.SaveAsync(); 

            var toReturn= mapper.Map<ResponsOpportunityParticipation>(participation);
            return CreatedAtAction(nameof(GetParticipationDetails), new { id = participation.Id }, toReturn);
        }

     

        [HttpDelete("{id}")]
       // [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteParticipation(int id)
        {
            var participation = await _unitOfWork.OpportunityParticipation.GetAsync(p => p.Id == id);
            if (participation == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            /*
            // Only the user who applied or an admin can delete it
            if (User.IsInRole("User") && userId != participation.AppUserId)
                return Forbid();
           */
            _unitOfWork.OpportunityParticipation.Delete(participation);
            await _unitOfWork.SaveAsync();

            return NoContent();
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
