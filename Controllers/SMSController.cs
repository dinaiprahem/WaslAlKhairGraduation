using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.DTOs.SMS;
using WaslAlkhair.Api.Services;
using WaslAlkhair.Api.Helpers;
using System.Net;

namespace WaslAlkhair.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SMSController : ControllerBase
    {
        private readonly ISMSService _smsService;
        private readonly ILogger<SMSController> _logger;

        public SMSController(ISMSService smsService, ILogger<SMSController> logger)
        {
            _smsService = smsService;
            _logger = logger;
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendDedicationSMS([FromBody] DedicationSMSDto smsRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _smsService.SendAsync(smsRequest.MobileNumber, smsRequest.Body);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Dedication SMS sent successfully to {MobileNumber}. MessageId: {MessageId}",
                        smsRequest.MobileNumber, result.MessageId);
                    return Ok(new APIResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        IsSuccess = true,
                        Message = "Dedication SMS sent successfully",
                        Result = result
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to send dedication SMS to {MobileNumber}. Error: {Error}",
                        smsRequest.MobileNumber, result.ErrorMessage);
                    return BadRequest(new APIResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { result.ErrorMessage ?? "Unknown error occurred" }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending dedication SMS");
                return StatusCode(500, new APIResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "An unexpected error occurred while sending SMS" }
                });
            }
        }

        [HttpPost("send-group")]
        public async Task<IActionResult> SendGroupMMS([FromBody] GroupMMSDto groupMmsRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _smsService.SendGroupMMS(
                    groupMmsRequest.Recipients,
                    groupMmsRequest.Text,
                    groupMmsRequest.MediaUrls,
                    groupMmsRequest.Subject
                );

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Group message sent successfully to {Recipients}. MessageId: {MessageId}",
                        string.Join(", ", groupMmsRequest.Recipients), result.MessageId);
                    return Ok(new APIResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        IsSuccess = true,
                        Message = "Group message sent successfully",
                        Result = result
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to send group message to {Recipients}. Error: {Error}",
                        string.Join(", ", groupMmsRequest.Recipients), result.ErrorMessage);
                    return BadRequest(new APIResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { result.ErrorMessage ?? "Unknown error occurred" }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending group message");
                return StatusCode(500, new APIResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "An unexpected error occurred while sending group message" }
                });
            }
        }

       
    }
}
