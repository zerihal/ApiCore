using ApiCore.Common.Interfaces;
using DocParser.DocSearch;
using DocParser.Factories;
using DocParser.Interfaces;
using DocumentFormat.OpenXml.Drawing.Diagrams;
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
        public async Task<IActionResult> ParseDocsForLinks([FromForm] List<IFormFile> files)
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
                        var fileKey = GetUniqueFileName(file.FileName, results.Keys);
                        var links = parser.GetDocLinks();
                        results.Add(fileKey, links);
                    }
                }
          
            }

            await Task.CompletedTask;
            return Ok(results);
        }

        [HttpPost("searchdocs")]
        public async Task<IActionResult> SearchDocs([FromForm] List<IFormFile> files, string searchString)
        {
            var results = new Dictionary<string, ISearchResult>();
            var streams = new List<Stream>();

            try
            {
                streams.AddRange(files.Select(f => f.OpenReadStream()));
                var searcher = new DocSearcher(streams);

                // ToDo: Doc searcher needs an update - should need some way to identify the results against the form file stream.
                // Currently will only return results where they are found in a file, which means for streams we will not have a
                // file name to match against - this will need some thought - maybe a file ID (simple int for order loaded and perhaps
                // GUID for unique ID) to allow files to matched against an input collection such as here? Another option may be
                // to add a simple search option for single file - Search and AdvancedSearch overloads that take a file and the
                // search string (this could be useful to add anyway actually!)

                while (!searcher.IsInitialised)
                    await Task.Delay(5);

                // ToDo: Populate results
            }
            finally
            {
                streams.ForEach(s => s.Dispose());
            }

            return Ok(results);
        }

        private string GetUniqueFileName(string filename, IEnumerable<string> existingFileNames)
        {
            if (existingFileNames == null || !existingFileNames.Contains(filename))
                return filename;

            var counter = 1;
            string newFileName;

            do
            {
                newFileName = $"{filename} ({counter})";
                counter++;
            }
            while (existingFileNames.Contains(newFileName));

            return newFileName;
        }
    }
}
