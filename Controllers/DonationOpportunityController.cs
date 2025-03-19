using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using WaslAlkhair.Api.Services;
using WaslAlkhair.Api.DTOs.Donation;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationOpportunityController : ControllerBase
    {
        private readonly IFileService _fileStorageService;
        private readonly AppDbContext _context;

        public DonationOpportunityController(IFileService fileStorageService, AppDbContext context)
        {
            _fileStorageService = fileStorageService;
            _context = context;
        }

        // ✅ GET: api/DonationOpportunity (Get all opportunities)
        [HttpGet]
        public async Task<IActionResult> GetAllOpportunities([FromQuery] OpportunityStatus? status = null)
        {
            var query = _context.DonationOpportunities
                .Include(o => o.Category)
                .Include(o => o.Charity)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status);
            }

            var opportunities = await query.ToListAsync();
            return Ok(opportunities);
        }

        // ✅ GET: api/DonationOpportunity/{id} (Get single opportunity)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOpportunityById(int id)
        {
            var opportunity = await _context.DonationOpportunities
                .Include(o => o.Category)
                .Include(o => o.Charity)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
                return NotFound("Donation Opportunity not found");

            // Increase page visits when retrieved
            opportunity.PageVisits++;
            await _context.SaveChangesAsync();

            return Ok(opportunity);
        }

        // ✅ POST: api/DonationOpportunity (Create a new opportunity)
        [HttpPost]
        public async Task<IActionResult> CreateOpportunity([FromForm] CreateDonationOpportunityDTO dto) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure the charity exists before assigning the opportunity
            var charity = await _context.Users.FindAsync(dto.CharityId);
            if (charity == null)
                return NotFound("Charity not found");

            var opportunity = new DonationOpportunity
            {
                CharityId = charity.Id,
                CategoryId = dto.CategoryId,
                Title=dto.Title,
                Description=dto.Description,
            };
            string? imagePath = null;
            if (dto.ImageUrl != null)
            {
                imagePath = await _fileStorageService.UploadFileAsync(
                    dto.ImageUrl,
                    "DonationOPP"
                );
            }

            opportunity.ImageUrl = imagePath;

            _context.DonationOpportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ✅ PUT: api/DonationOpportunity/{id} (Update opportunity)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOpportunity(int id, [FromBody] DonationOpportunity updatedOpportunity)
        {
            var opportunity = await _context.DonationOpportunities.FindAsync(id);
            if (opportunity == null)
                return NotFound("Opportunity not found");

            // Update only the allowed fields
            opportunity.Title = updatedOpportunity.Title;
            opportunity.Description = updatedOpportunity.Description;
            opportunity.TargetAmount = updatedOpportunity.TargetAmount;
            opportunity.Deadline = updatedOpportunity.Deadline;
            opportunity.Status = updatedOpportunity.Status;
            opportunity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(opportunity);
        }

        // ✅ DELETE: api/DonationOpportunity/{id} (Delete opportunity)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOpportunity(int id)
        {
            var opportunity = await _context.DonationOpportunities.FindAsync(id);
            if (opportunity == null)
                return NotFound("Opportunity not found");

            _context.DonationOpportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            return Ok("Opportunity deleted successfully");
        }
    }
}
