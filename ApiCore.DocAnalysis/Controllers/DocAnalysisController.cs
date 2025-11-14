using ApiCore.Common.Interfaces;
using DocParser.Factories;
using DocParser.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCore.DocAnalysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocAnalysisController : ControllerBase, IApiModule
    {
        private IList<IDocParser> DocParsers = new List<IDocParser>();

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

        [HttpPost("parselinks")]
        public async Task<IActionResult> ParseDocForLinks([FromForm] List<IFormFile> files)
        {
            var results = new Dictionary<string, IEnumerable<string>>();

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLower();

                if (ext != null)
                {
                    // Get the appropriate doc parser for this file type
                    var parser = DocParsers.FirstOrDefault(p => p.IsApplicableForFile(ext));

                    // No appropriate doc parser created yet for this file type - try and create one and add it to collection
                    // of parsers to reuse for further files of the same type.
                    if (parser == null)
                    {
                        parser = DocParserFactory.CreateDocParserForFile(file.FileName);

                        if (parser != null)
                            DocParsers.Add(parser);
                    }

                    // If parser is still null at this point, no appropriate parser is available for this file type.
                    if (parser == null)
                    {
                        // Unsupported file type
                        continue;
                    }

                    // Open file stream, parse for links, and add to results.
                    using var fs = file.OpenReadStream();
                    
                    if (parser.LoadFile(fs))
                    {
                        var links = parser.GetDocLinks();
                        results.Add(file.Name, links);
                    }
                }
          
            }

            await Task.CompletedTask;
            return Ok(results);
        }
    }
}
