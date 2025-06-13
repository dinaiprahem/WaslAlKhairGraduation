using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs.Reviews;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        private readonly APIResponse _response;

        public ReviewsController(UserManager<AppUser> userManager, AppDbContext db, APIResponse response)
        {
            _userManager = userManager;
            _db = db;
            _response = response;
        }

        /// <summary>
        /// Add a review from one user to another
        /// </summary>
        [Authorize]
        [HttpPost]
        [SwaggerOperation(Summary = "Add a review for another user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        public async Task<IActionResult> AddReview([FromBody] CreateUserReviewDto dto)
        {
            try
            {
                var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (reviewerId == dto.TargetUserId)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("You cannot review yourself.");
                    return BadRequest(_response);
                }

                var targetUser = await _userManager.FindByIdAsync(dto.TargetUserId);
                if (targetUser == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Target user not found.");
                    return NotFound(_response);
                }

                var existingReview = await _db.UserReviews
                    .FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.TargetUserId == dto.TargetUserId);

                if (existingReview != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("You have already submitted a review for this user.");
                    return BadRequest(_response);
                }

                var review = new UserReview
                {
                    ReviewerId = reviewerId,
                    TargetUserId = dto.TargetUserId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _db.UserReviews.Add(review);
                await _db.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Message = "Review submitted successfully.";
                _response.Result = review;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)_response.StatusCode, _response);
            }
        }

        /// <summary>
        /// Get all reviews written for a specific user
        /// </summary>
        [HttpGet("{userId}")]
        [SwaggerOperation(Summary = "Get reviews for a specific user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        public async Task<IActionResult> GetUserReviews(string userId)
        {
            try
            {
                var reviews = await _db.UserReviews
                    .Include(r => r.Reviewer)
                    .Where(r => r.TargetUserId == userId)
                    .ToListAsync();

                if (!reviews.Any())
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No reviews found for this user.");
                    return NotFound(_response);
                }

                var reviewDtos = reviews.Select(r => new GetUserReviewsDTO
                {
                    UserName = r.Reviewer.FullName,
                    UserImageUrl = r.Reviewer.image,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = reviewDtos;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)_response.StatusCode, _response);
            }
        }
    }
}
