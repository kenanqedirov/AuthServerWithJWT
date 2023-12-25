using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace Api2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InVoiceController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInVoices()
        {
            var username = HttpContext.User.Identity.Name;
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return Ok($"InVoice => Username : {username}-UserId:{userIdClaim.Value}");
        }
    }
}
