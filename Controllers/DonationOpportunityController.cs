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
                    dto.ImageUrl,
                    "DonationOPP"
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
                    await fileStorageService.DeleteFileAsync(opportunity.ImageUrl, "DonationOPP");

                }
                // ارفع الصورة إلى Cloudinary أو أي خدمة صور
                var imageUrl = await fileStorageService.UploadFileAsync(dto.Image , "DonationOPP");
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

            // Increase page visits when retrieved
            opportunity.PageVisits++;

            var returnOpportunity=_mapper.Map<ResponseDonationOpportunityDetailsDTO>(opportunity);
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

               
        



    }
}
