using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs;
using WaslAlkhair.Api.DTOs.Donation;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public DonationController(
            AppDbContext context,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get all donations made by the authenticated user",
            Description = "Retrieves a list of donations made by the currently logged-in user."
        )]
        public async Task<ActionResult<IEnumerable<DonationDto>>> GetUserDonations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            var donations = await _context.Donations
                .Where(d => d.DonorId == userId)
                .Include(d => d.Category)
                .Include(d => d.Distributions)
                    .ThenInclude(dd => dd.DonationOpportunity)
                .Include(d => d.GiftDonation)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<DonationDto>>(donations));
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get a specific donation by ID",
            Description = "Retrieves details of a specific donation using its unique ID."
        )]
        public async Task<ActionResult<DonationDto>> GetDonation(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.Category)
                .Include(d => d.Distributions)
                    .ThenInclude(dd => dd.DonationOpportunity)
                .Include(d => d.GiftDonation)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donation == null)
                return NotFound();

            return _mapper.Map<DonationDto>(donation);
        }

        [HttpPost("opportunity")]
        [SwaggerOperation(
            Summary = "Donate to a specific opportunity",
            Description = "Allows user to donate to a specific donation opportunity."
        )]
        public async Task<ActionResult<DonationDto>> DonateToOpportunity(OpportunityDonationDto donationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var opportunity = await _context.DonationOpportunities
                .FirstOrDefaultAsync(o => o.Id == donationDto.OpportunityId);

            if (opportunity == null)
                return NotFound("Donation opportunity not found");

            if (opportunity.Status != OpportunityStatus.Active)
                return BadRequest("This donation opportunity is not active");

            // Create the donation
            var donation = new Donation
            {
                Amount = donationDto.Amount,
                DonorId = userId,
                Type = DonationType.Opportunity,
                DonatedAt = DateTime.UtcNow
            };

            // Handle gift donation if applicable
            if (donationDto.IsGift && donationDto.GiftDetails != null)
            {
                donation.Type = DonationType.Gifted;
                donation.GiftDonation = _mapper.Map<GiftDonation>(donationDto.GiftDetails);
            }

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Create distribution record
            var distribution = new DonationDistribution
            {
                DonationId = donation.Id,
                DonationOpportunityId = opportunity.Id,
                DistributedAmount = donationDto.Amount
            };

            _context.DonationDistributions.Add(distribution);

            // Update opportunity statistics
            opportunity.CollectedAmount += donationDto.Amount;
            opportunity.NumberOfDonors += 1;
            opportunity.UpdatedAt = DateTime.UtcNow;

            // Check if target amount is reached
            if (opportunity.TargetAmount.HasValue && opportunity.CollectedAmount >= opportunity.TargetAmount.Value)
            {
                opportunity.Status = OpportunityStatus.Completed;
            }

            await _context.SaveChangesAsync();

            // Return the created donation
            return CreatedAtAction(
                nameof(GetDonation),
                new { id = donation.Id },
                _mapper.Map<DonationDto>(donation)
            );
        }

        [HttpPost("category")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Donate to a specific category",
            Description = "Allows the authenticated user to donate to a specific donation category. The amount is distributed among active opportunities in the category."
        )]
        public async Task<ActionResult<DonationDto>> DonateToCategory(CategoryDonationDto donationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.DonationCategories
                .Include(c => c.DonationOpportunities)
                .FirstOrDefaultAsync(c => c.Id == donationDto.CategoryId);

            if (category == null)
                return NotFound("Donation category not found");

            // Get active opportunities in this category
            var activeOpportunities = category.DonationOpportunities
                .Where(o => o.Status == OpportunityStatus.Active)
                .ToList();

            if (!activeOpportunities.Any())
                return BadRequest("No active opportunities in this category");

            // Create the donation
            var donation = new Donation
            {
                Amount = donationDto.Amount,
                DonorId = userId,
                Type = DonationType.Category,
                CategoryId = category.Id,
                DonatedAt = DateTime.UtcNow
            };

            // Handle gift donation if applicable
            if (donationDto.IsGift && donationDto.GiftDetails != null)
            {
                donation.Type = DonationType.Gifted;
                donation.GiftDonation = _mapper.Map<GiftDonation>(donationDto.GiftDetails);
            }

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Distribute the amount among active opportunities in the category
            await DistributeAmongOpportunities(donation, activeOpportunities);

            // Return the created donation
            return CreatedAtAction(
                nameof(GetDonation),
                new { id = donation.Id },
                _mapper.Map<DonationDto>(donation)
            );
        }

        [HttpPost("fast")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Make a fast donation",
            Description = "Allows the authenticated user to make a donation that is distributed randomly among all active opportunities."
        )]
        public async Task<ActionResult<DonationDto>> FastDonate(FastDonationDto donationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get all active opportunities
            var activeOpportunities = await _context.DonationOpportunities
                .Where(o => o.Status == OpportunityStatus.Active)
                .ToListAsync();

            if (!activeOpportunities.Any())
                return BadRequest("No active opportunities available");

            // Create the donation
            var donation = new Donation
            {
                Amount = donationDto.Amount,
                DonorId = userId,
                Type = DonationType.Fast,
                DonatedAt = DateTime.UtcNow
            };

            // Handle gift donation if applicable
            if (donationDto.IsGift && donationDto.GiftDetails != null)
            {
                donation.Type = DonationType.Gifted;
                donation.GiftDonation = _mapper.Map<GiftDonation>(donationDto.GiftDetails);
            }

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Distribute the amount randomly among all active opportunities
            await DistributeAmongOpportunities(donation, activeOpportunities);

            // Return the created donation
            return CreatedAtAction(
                nameof(GetDonation),
                new { id = donation.Id },
                _mapper.Map<DonationDto>(donation)
            );
        }

        // Helper method to distribute donation among multiple opportunities
        private async Task DistributeAmongOpportunities(Donation donation, List<DonationOpportunity> opportunities)
        {
            // Implement distribution logic (equal, weighted, or priority-based)
            decimal amountPerOpportunity = donation.Amount / opportunities.Count;

            foreach (var opportunity in opportunities)
            {
                var distribution = new DonationDistribution
                {
                    DonationId = donation.Id,
                    DonationOpportunityId = opportunity.Id,
                    DistributedAmount = amountPerOpportunity
                };

                _context.DonationDistributions.Add(distribution);

                // Update opportunity statistics
                opportunity.CollectedAmount += amountPerOpportunity;
                opportunity.NumberOfDonors += 1;
                opportunity.UpdatedAt = DateTime.UtcNow;

                // Check if target amount is reached
                if (opportunity.TargetAmount.HasValue && opportunity.CollectedAmount >= opportunity.TargetAmount.Value)
                {
                    opportunity.Status = OpportunityStatus.Completed;
                }
            }

            await _context.SaveChangesAsync();
        }
		//get all user donations 
		[HttpGet("my-donations/summary")]
		[Authorize]
		public async Task<ActionResult> GetMyDonationSummary()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();
            //without is paid 
			var donations = await _context.Donations
				.Where(d => d.DonorId == userId )
				.ToListAsync();

			var totalAmount = donations.Sum(d => d.Amount);
			var count = donations.Count;

			return Ok(new
			{
				TotalAmount = totalAmount,
				DonationCount = count
			});
		}


	}
}