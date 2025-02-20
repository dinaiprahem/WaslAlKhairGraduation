using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _unitOfWork.Users.GetAsync(o=>o.Id==id);
            if (user == null)
                return NotFound("User not found.");

            _unitOfWork.Users.Delete(user);

            var success = await _unitOfWork.CompleteAsync();
            if (!success)
                return BadRequest("Failed to delete user.");

            return Ok("Deleted Succefuly");
        }
    }
}
