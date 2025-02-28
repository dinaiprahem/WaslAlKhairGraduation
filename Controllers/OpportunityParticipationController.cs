using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpportunityParticipationController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        public OpportunityParticipationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OpportunityParticipation>>> GetAll()
        {
            var participations = await _unitOfWork.OpportunityParticipation.GetAllAsync();
            return Ok(participations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OpportunityParticipation>> GetById(int id)
        {
            var participation = await _unitOfWork.OpportunityParticipation.GetAsync(p => p.Id == id);
            if (participation == null)
                return NotFound(new { message = "Participation not found" });

            return Ok(participation);
        }

        [HttpPost]
        public async Task<ActionResult> Create(OpportunityParticipation participation)
        {
            await _unitOfWork.OpportunityParticipation.CreateAsync(participation);
            await _unitOfWork.SaveAsync(); // Commit transaction
            return CreatedAtAction(nameof(GetById), new { id = participation.Id }, participation);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var participation = await _unitOfWork.OpportunityParticipation.GetAsync(p => p.Id == id);
            if (participation == null)
                return NotFound(new { message = "Participation not found" });

            _unitOfWork.OpportunityParticipation.Delete(participation);
            await _unitOfWork.SaveAsync(); // Commit transaction
            return NoContent();
        }
    }
}
