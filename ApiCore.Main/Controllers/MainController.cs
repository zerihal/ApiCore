using ApiCore.Common;
using ApiCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiCore.Main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        private readonly IEnumerable<IApiModule> _modules;

        /// <summary>
        /// Default constructor that loads <see cref="IApiModule"/> implementations from service DI.
        /// </summary>
        /// <param name="modules">Sub APIs implementations of <see cref="IApiModule"/>.</param>
        public MainController(IEnumerable<IApiModule> modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// Gets the status of the main API and loaded modules.
        /// </summary>
        /// <returns>API status information.</returns>
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

        /// <summary>
        /// Gets all available endpoints from sub APIs.
        /// </summary>
        /// <returns>Details of all endpoints available to the core service (<see cref="EndpointInfo"/>).</returns>
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
