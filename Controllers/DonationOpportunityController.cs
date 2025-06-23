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
using WaslAlkhair.Api.DTOs.Opportunity;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Repositories.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationOpportunityController : ControllerBase
    {
        private readonly IFileService fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DonationCategoryController> _logger;
        private readonly APIResponse _response;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public DonationOpportunityController(IFileService fileStorageService, IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DonationCategoryController> logger,
            AppDbContext context,
            APIResponse response)
        {
            this.fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _response = response;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Charity")]
        public async Task<IActionResult> CreateOpportunity([FromForm] CreateDonationOpportunityDTO dto)
        {
            // Ensure category is exist
            var category = await _unitOfWork.DonationCategory
                .GetAsync(c => c.Name == dto.CategoryName && !c.IsDeleted);

            if (category == null) return BadRequest("Category not found.");

            // Get charityId 
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            var charityId = claim.Value;

            // ImageUrl
            string? imagePath = null;
            if (dto.ImageUrl != null)
            {
                imagePath = await fileStorageService.UploadFileAsync(
                    dto.ImageUrl
                );
            }

           
            var opportunity = _mapper.Map<DonationOpportunity>(dto);
            opportunity.ImageUrl = imagePath;
            opportunity.CategoryId = category.Id;
            opportunity.CharityId = charityId;
            opportunity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.DonationOpportunity.CreateAsync(opportunity);
            await _unitOfWork.SaveAsync();

            return Ok("Opportunity created successfully.");
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Charity")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var opportunity = await _unitOfWork.DonationOpportunity.GetAsync(o=>o.Id==id);
            if (opportunity == null) return NotFound();

            // Get charityId 
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            var charityId = claim.Value;

            if (charityId != opportunity.CharityId) return BadRequest("You are Only allowed to Delete your opportunities");

            opportunity.Status = OpportunityStatus.Closed;
            await _unitOfWork.SaveAsync();

            return Ok("Opportunity marked as Closed.");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Charity")]
        public async Task<IActionResult> UpdateOpportunity(int id, [FromForm] UpdateDonationOpportunityDTO dto)
        {
            var opportunity = await _unitOfWork.DonationOpportunity.GetAsync(o => o.Id == id);
            if (opportunity == null)
                return NotFound("Opportunity not found.");

            // Get charityId 
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            var charityId = claim.Value;

            if (charityId != opportunity.CharityId) return BadRequest("You are Only allowed to Update your opportunities");
            _mapper.Map(dto, opportunity);

            // ✅ تحديث صورة لو موجودة
            if (dto.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(opportunity.ImageUrl))
                {
                    await fileStorageService.DeleteFileAsync(opportunity.ImageUrl);

                }
                // ارفع الصورة إلى Cloudinary أو أي خدمة صور
                var imageUrl = await fileStorageService.UploadFileAsync(dto.Image );
                opportunity.ImageUrl = imageUrl;
            }

            // ✅ تحديث التواريخ
            opportunity.UpdatedAt = DateTime.UtcNow;

            // ✅ تحقق من حالة الفرصة حسب التواريخ والمبالغ
            if (opportunity.Deadline.HasValue && opportunity.Deadline.Value < DateTime.UtcNow)
            {
                opportunity.Status = OpportunityStatus.Closed;
            }
            else if (opportunity.TargetAmount.HasValue && opportunity.CollectedAmount >= opportunity.TargetAmount)
            {
                opportunity.Status = OpportunityStatus.Completed;
            }

            await _unitOfWork.SaveAsync();
            return Ok("Opportunity updated successfully.");
        }

        // Get single opportunity with details 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOpportunityById(int id)
        {
            var opportunity = await _unitOfWork.DonationOpportunity
                .GetAsync(o => o.Id == id);

            if (opportunity == null)
                return NotFound("Donation Opportunity not found");

            // Get role
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);

            if (claim == null || claim.Value == "User")
            {
                opportunity.PageVisits++;
            }

            // Get the most recent donation for this opportunity
            var lastDonation = await _context.DonationDistributions
                .Where(dd => dd.DonationOpportunityId == id)
                .Include(dd => dd.Donation)
                .OrderByDescending(dd => dd.Donation.DonatedAt)
                .FirstOrDefaultAsync();

            var returnOpportunity = _mapper.Map<ResponseDonationOpportunityDetailsDTO>(opportunity);

            // Calculate LastDonationAgo
            if (lastDonation != null)
            {
                returnOpportunity.LastDonationAgo = CalculateTimeAgo(lastDonation.Donation.DonatedAt);
            }
            else
            {
                returnOpportunity.LastDonationAgo = "No donations yet";
            }

            // Calculate completion percentage
            returnOpportunity.CompletionPercentage = CalculateCompletionPercentage(opportunity.CollectedAmount, opportunity.TargetAmount);

            await _context.SaveChangesAsync();

            return Ok(returnOpportunity);
        }
    
            




        // ✅ GET: api/DonationOpportunity (Get all opportunities)
        [HttpGet]
        public async Task<IActionResult> GetAllOpportunities([FromQuery] OpportunityStatus? status = null , [FromQuery] string? charityId = null)
        {
            var query = _context.DonationOpportunities
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status);
            }

            if (charityId!=null)
            {
                query = query.Where(o => o.CharityId == charityId);
            }

            var opportunities = await query.ToListAsync();

            var returnOpportunities = _mapper.Map<List<ResponseAllDonationOpportunities>>(opportunities);

            return Ok(returnOpportunities);
        }




        private string CalculateTimeAgo(DateTime donationDate)
        {
            var timeSpan = DateTime.UtcNow - donationDate;

            if (timeSpan.Days > 365)
            {
                int years = timeSpan.Days / 365;
                return years == 1 ? "منذ سنة واحدة" : $"منذ {years} سنوات";
            }
            else if (timeSpan.Days > 30)
            {
                int months = timeSpan.Days / 30;
                return months == 1 ? "منذ شهر واحد" : $"منذ {months} أشهر";
            }
            else if (timeSpan.Days > 0)
            {
                return timeSpan.Days == 1 ? "منذ يوم واحد" : $"منذ {timeSpan.Days} أيام";
            }
            else if (timeSpan.Hours > 0)
            {
                return timeSpan.Hours == 1 ? "منذ ساعة واحدة" : $"منذ {timeSpan.Hours} ساعات";
            }
            else if (timeSpan.Minutes > 0)
            {
                return timeSpan.Minutes == 1 ? "منذ دقيقة واحدة" : $"منذ {timeSpan.Minutes} دقائق";
            }
            else
            {
                return "منذ لحظات";
            }
        }

        // 3. Helper method to calculate completion percentage
        private decimal? CalculateCompletionPercentage(decimal collectedAmount, decimal? targetAmount)
        {
            if (!targetAmount.HasValue || targetAmount.Value == 0)
                return null;

            var percentage = (collectedAmount / targetAmount.Value) * 100;
            return Math.Round(percentage, 2); // Round to 2 decimal places
        }
    }

}

