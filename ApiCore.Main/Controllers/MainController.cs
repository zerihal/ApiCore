using Microsoft.AspNetCore.Mvc;

namespace ApiCore.Main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Api = "Main",
                Status = "Active",
                Message = "Main API is running."
            });
        }
    }
}
