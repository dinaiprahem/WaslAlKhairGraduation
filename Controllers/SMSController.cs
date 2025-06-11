using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.DTOs.SMS;
using WaslAlkhair.Api.Services;

namespace WaslAlkhair.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SMSController : ControllerBase
    {
        private readonly ISMSService _smsService;
        //private readonly ILogger<SMSController> _logger;

        public SMSController(ISMSService smsService) //, ILogger<SMSController> logger
        {
            _smsService = smsService;
            // _logger = logger;
        }

        [HttpPost("send-dedication")]
        public IActionResult SendDedicationSMS([FromBody] DedicationSMSDto smsRequest)
        {
            var result = _smsService.Send(smsRequest.MobileNumber, smsRequest.Body);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
                return BadRequest(result.ErrorMessage);
            return Ok(result);
        }
    }
}

