using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Controllers
{
    [ApiController]
    [Route("api")]
    public class AppController : ControllerBase
    {

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { Status = "App is running", Timestamp = DateTime.UtcNow });
        }
    }
}