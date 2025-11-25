using ApiCore.Common.Interfaces;
using AssemblyDependencyAnalyser.CommonInterfaces;
using AssemblyDependencyAnalyser.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCore.CodeAnalysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeAnalysisController : ControllerBase, IApiModule
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Api = "CodeAnalysis",
                Status = "Active",
                Message = "Placeholder endpoint for code analysis API."
            });
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSingleAssemblies([FromForm] List<IFormFile> files)
        {
            var results = new List<IAnalysedFile>();
            var analyser = new DependencyAnalyser();

            foreach (var file in files)
            {
                results.Add(analyser.AnalyseAssembly(file.OpenReadStream()));
            }

            await Task.CompletedTask;
            return Ok(results);
        }
    }
}
