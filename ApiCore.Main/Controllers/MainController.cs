using ApiCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiCore.Main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        private readonly IEnumerable<IApiModule> _modules;

        public MainController(IEnumerable<IApiModule> modules)
        {
            _modules = modules;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Api = "Main",
                Status = "Active",
                Message = "Main API is running.",
                ModulesLoaded = _modules.Count().ToString()
            });
        }

        [HttpGet("endpoints")]
        public IActionResult GetEndpoints()
        {
            var endpoints = _modules
                .SelectMany(m => m.GetApiEndpoints())
                .ToList();

            return Ok(endpoints);
        }
    }
}
