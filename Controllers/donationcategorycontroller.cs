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
using Swashbuckle.AspNetCore.Annotations;
using AutoMapper;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using WaslAlkhair.Api.Helpers;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.Authorization;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationCategoryController : ControllerBase
    {
        private readonly IFileService fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DonationCategoryController> _logger;
        private readonly APIResponse _response;

        public DonationCategoryController(
            IFileService fileStorageService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DonationCategoryController> logger,
            APIResponse response)
        {
            this.fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _response = response;
        }

        // Get all categories
        [HttpGet]
        [SwaggerOperation(Summary = "Get All Donation Categories")]
        [ResponseCache(Duration = 60)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> GetAllCategories()
        {
            try
            {
                _logger.LogInformation("Fetching all donation categories");
                var categories = await _unitOfWork.DonationCategory.GetAllAsync(c => c.IsDeleted==false);
                var returnCategory = _mapper.Map<List<ResponseDonationCategoryDTO>>(categories);
                
                _response.Result = returnCategory;
                _response.Message = "Categories retrieved successfully";
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching donation categories");
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)_response.StatusCode, _response);
            }
        }


        // Get single category
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get a specific Category with its Active opportunities")]
        [ResponseCache(Duration = 30)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> GetCategoryById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching donation category with ID: {Id}", id);
                var category = await _unitOfWork.DonationCategory.GetCategoryWithOpportunitiesAsync(o => o.Id == id);
                
                if (category == null)
                {
                    _logger.LogWarning("Category not found with ID: {Id}", id);
                    _response.IsSuccess = false;
                    _response.Message = "Category not found";
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = category;
                _response.Message = "Category retrieved successfully";
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching category with ID: {Id}", id);
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)_response.StatusCode, _response);
            }
        }

        // Create a new category
        [HttpPost]
        [SwaggerOperation(Summary = "Admin add a new Category")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> CreateCategory([FromForm] CreateDonationCategoryDTO dto)
        {
            _logger.LogInformation("Creating new donation category");
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var category = new DonationCategory
                {
                    Name = dto.Name,
                    Description = dto.Description,
                };

                string? imagePath = null;
                if (dto.ImageUrl != null)
                {
                    imagePath = await fileStorageService.UploadFileAsync(
                        dto.ImageUrl
                    );
                }

                category.ImageUrl = imagePath;

                await _unitOfWork.DonationCategory.CreateAsync(category);
                await _unitOfWork.SaveAsync();

                _response.Result = category;
                _response.Message = "Category created successfully";
                _response.StatusCode = HttpStatusCode.Created;
                return StatusCode((int)_response.StatusCode, _response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching category");
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)_response.StatusCode, _response);
            }
        }

        // Update category
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Admin update a category")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> UpdateCategory(int id, [FromForm] CreateDonationCategoryDTO updatedCategory)
        {
            _logger.LogInformation("Updating donation category with ID: {Id}", id);
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var category = await _unitOfWork.DonationCategory.GetAsync(o => o.Id == id);
                if (category == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Category not found";
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                category.Name = updatedCategory.Name;
                category.Description = updatedCategory.Description;
                if (updatedCategory.ImageUrl != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        await fileStorageService.DeleteFileAsync(category.ImageUrl );
                       
                    }
                    // Upload new image
                    category.ImageUrl = await fileStorageService.UploadFileAsync(
                        updatedCategory.ImageUrl
                    );
                }

                await _unitOfWork.SaveAsync();
                
                _response.Result = category;
                _response.Message = "Category updated successfully";
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching category with ID: {Id}", id);
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)_response.StatusCode, _response);
            }
        }

        // Delete a category
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Admin delete a category")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> DeleteCategory(int id)
        {
            _logger.LogInformation("Deleting donation category with ID: {Id}", id);
            try
            {
                var category = await _unitOfWork.DonationCategory.GetAsync(o=>o.Id==id);
                if (category == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Category not found";
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                category.IsDeleted = true;
                await _unitOfWork.SaveAsync();

                _response.Message = "Category soft deleted successfully";
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching category with ID: {Id}", id);
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)_response.StatusCode, _response);
            }
        }
    }
}
