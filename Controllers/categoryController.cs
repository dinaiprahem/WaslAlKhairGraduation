using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaslAlkhair.Api.DTOs.Donation;
using static System.Net.Mime.MediaTypeNames;
using WaslAlkhair.Api.Services;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationCategoryController : ControllerBase
    {
        private readonly IFileService fileStorageService;
        private readonly AppDbContext _context;
        private readonly IRepositery<DonationCategory> repo;

        public DonationCategoryController(IFileService fileStorageService, AppDbContext context ,
            IRepositery<DonationCategory> repo)
        {
            this.fileStorageService = fileStorageService;
            _context = context;
            this.repo = repo;
        }

        // ✅ GET: api/DonationCategory (Get all categories)
        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] string? name = null)
        {
            var query = _context.DonationCategories.Include(c => c.DonationOpportunities).AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            var categories = await query.ToListAsync();
            return Ok(categories);
        }

        // ✅ GET: api/DonationCategory/{id} (Get single category)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.DonationCategories
                .Include(c => c.DonationOpportunities)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound("Category not found");

            return Ok(category);
        }

        // ✅ POST: api/DonationCategory (Create a new category)
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromForm]  CreateCategoryDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new DonationCategory
            {
                Name = dto.Name,
                Description = dto.Description,
            };

            string? imagePath = null;
            if (dto.ImageUrl != null)
            {
                imagePath = await fileStorageService.UploadFileAsync(
                    dto.ImageUrl,
                    "DonationOPP"
                );
            }

            category.ImageUrl = imagePath;

            _context.DonationCategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ✅ PUT: api/DonationCategory/{id} (Update category)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] DonationCategory updatedCategory)
        {
            var category = await _context.DonationCategories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            category.Name = updatedCategory.Name;
            category.Description = updatedCategory.Description;
            category.ImageUrl = updatedCategory.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await repo.GetAsync(o=>o.Id==id);
            if (category == null)
                return NotFound("Category not found");

            repo.Delete(category);
            await _context.SaveChangesAsync();

            return Ok("Category deleted successfully");
        }
    }
}
