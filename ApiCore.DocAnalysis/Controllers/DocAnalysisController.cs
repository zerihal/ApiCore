using ApiCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiCore.DocAnalysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocAnalysisController : ControllerBase, IApiModule
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Api = "DocAnalysis",
                Status = "Active",
                Message = "Placeholder endpoint for document analysis API."
            });
        }
    }
}
