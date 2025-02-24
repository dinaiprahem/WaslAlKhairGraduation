using System.Net;
using AutoMapper;
using Azure;
using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.DTOs.Charity;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories;
using WaslAlkhair.Api.Repositories.Interfaces;
using WaslAlkhair.Api.Services;
using static Google.Apis.Requests.BatchRequest;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharityController : ControllerBase
    {
        private readonly IFileService _fileStorageService;
        private readonly IUserRepository _AppuserRepository;
        private readonly APIResponse _response;
        private readonly IMapper _mapper;

        public CharityController( IFileService fileStorageService,
            IUserRepository userRepository, APIResponse response,
            IMapper mapper)
        {
            _fileStorageService = fileStorageService;
            _AppuserRepository = userRepository;
            _response = response;
            _mapper = mapper;
        }



        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> GetAllCharites()
        {
            try
            {
                var charities = await _AppuserRepository.GetAllCharitesAsync();

                if (charities == null || !charities.Any())
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No charities found.");
                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = charities;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpGet("{id}" , Name =("getCharityById"))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> GetCharityById([FromRoute] string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("ID is required.");
                    return BadRequest(_response);
                }


                var charity = await _AppuserRepository.GetUserByIdAsync(id);

                if (charity == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Charity not found.");
                    return NotFound(_response);
                }

                var charityToReturn = _mapper.Map<CharityDTO>(charity);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = charityToReturn;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> CreateCharity([FromForm] CreateCharityDto request)
        {
            try
            {
                // Check if Charity already exists
                var existingUser = await _AppuserRepository.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Charity is already exist.");
                    return BadRequest(_response);
                }

                // Upload image 
                string? imagePath = null;
                if (request.Image != null)
                {
                    imagePath = await _fileStorageService.UploadFileAsync(
                        request.Image,
                        "charity-logos"
                    );
                }

                // Create Charity
                var charty = _mapper.Map<AppUser>(request);
                charty.image = imagePath;

                var createResult = await _AppuserRepository.CreateUserAsync(charty, request.Password);
                if (!createResult.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = createResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }

                // Add charity to Role
                var roleResult = await _AppuserRepository.AddUserToRoleAsync(charty, "Charity");
                if (!roleResult.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = roleResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.Created;
                _response.Message = "Charity Added Successfly!";
                return CreatedAtRoute("getCharityById", new { id = charty.Id }, _response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }

        }

       

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        public async Task<ActionResult<APIResponse>> UpdateCharity([FromRoute] string id, [FromForm] UpdateCharityDTO dto)
        {
            try
            {
                if (dto == null || dto.Id != id)
                {
                    return BadRequest(new APIResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { " request is not valid " }
                    });
                }
                var charity = await _AppuserRepository.GetUserByIdAsync(id);
                if (charity == null)
                    return NotFound(new APIResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Charity not found." }
                    });
                
                bool updated = await _AppuserRepository.UpdateCharityAsync(dto, charity);
                if (!updated)
                    return BadRequest(new APIResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Update failed." }
                    });

                return Ok(new APIResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Message = "Charity updated successfully."
                });
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }

        }

        [HttpDelete("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(APIResponse))]
        public async Task<ActionResult> DeleteCharity([FromRoute] string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Charity ID is required.");
                return BadRequest(_response);
            }
            try
            {
                var Charity = await _AppuserRepository.GetUserByIdAsync(Id);
                if (Charity == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Charity not found ");
                    return NotFound(_response);
                }
                bool deletResult = await _AppuserRepository.DeleteUserAsync(Id);
                if (!deletResult)
                {
                    _response.StatusCode = HttpStatusCode.InternalServerError;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Delete failed. Please try again.");
                    return StatusCode(500, _response);
                }
                if (Charity.image != null)
                {
                    await _fileStorageService.DeleteFileAsync(Charity.image, "charity-logos");
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Deleted Succufly";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }
    }
}

