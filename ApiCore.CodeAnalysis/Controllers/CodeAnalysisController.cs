using AssemblyDependencyAnalyser.CommonInterfaces;
using AssemblyDependencyAnalyser.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCore.CodeAnalysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeAnalysisController : ControllerBase
    {
        /// <summary>
        /// Gets the status of the API (returning active as default).
        /// </summary>
        /// <returns>Active status.</returns>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Api = "CodeAnalysis",
                Status = "Active",
                Message = "Endpoint for code analysis API."
            });
        }

        /// <summary>
        /// Analyses one or more assembly files.
        /// </summary>
        /// <param name="files">Files to analyse.</param>
        /// <returns>Analysed files as JSON object.</returns>
        [HttpPost("analyse")]
        public async Task<IActionResult> AnalyseAssemblies([FromForm] List<IFormFile> files)
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

        /// <summary>
        /// Analyses a project from archive (i.e. a zipped folder containing all project files and assemblies).
        /// </summary>
        /// <param name="file">Archive to analyse.</param>
        /// <returns>Analysed project assemblies as JSON object.</returns>
        [HttpPost("analyseproject")]
        public async Task<IActionResult> AnalyseProjectArchive([FromForm] List<IFormFile> file)
        {
            if (file.FirstOrDefault() is IFormFile project && project.Length > 0)
            {
                // Create a unique temp file with same extension
                var tempPath = Path.Combine(Path.GetTempPath(),$"{Guid.NewGuid()}{Path.GetExtension(project.FileName)}");

                try
                {
                    // Copy IFormFile to disk
                    using (var stream = new FileStream(tempPath, FileMode.Create))
                    {
                        await project.CopyToAsync(stream);
                    }

                    // Pass the file path to your analyser
                    var analyser = new DependencyAnalyser();
                    var result = analyser.AnalyseApplicationArchive(tempPath);

                    return Ok(result);
                }
                finally
                {
                    // Cleanup
                    if (System.IO.File.Exists(tempPath))
                        System.IO.File.Delete(tempPath);
                }
            }
            else
            {
                return BadRequest("Input file is null or empty");
            }
        }
    }
}
