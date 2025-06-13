using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.DTOs;
using WaslAlkhair.Api.Services;

namespace WaslAlkhair.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<ActionResult<SearchResultDto>> Search([FromQuery] string query)
        {
            try
            {
                var results = await _searchService.SearchAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching", error = ex.Message });
            }
        }
    }
} 