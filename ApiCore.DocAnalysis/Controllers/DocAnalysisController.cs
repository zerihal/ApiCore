using ApiCore.Common.Interfaces;
using DocParser.DocSearch;
using DocParser.ExtensionMethods;
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
            var results = new Dictionary<string, IList<ISearchResult>>();
            IEnumerable<IFormFileStream>? fileStreams = null;

            try
            {
                // Convert form files to form file streams for the DocSearcher
                fileStreams = files.ToFormFileStreams();
                var searcher = new DocSearcher(fileStreams);

                // Make sure searcher is initialised (break out if takes too long to prevent continuous
                // loop - 10 seconds should be more than enough time for this).
                var maxDelay = 10000;
                var delayCount = 0;

                while (!searcher.IsInitialised)
                {
                    if (delayCount > maxDelay)
                        break;

                    await Task.Delay(5);
                    delayCount += 5;
                }

                // Assuming searcher has been initialised, perform the search and structure results into
                // a dictionary of file names and collective results to be returned.
                if (searcher.IsInitialised)
                {
                    var searchResults = await searcher.Search(searchString);

                    foreach (var searchResult in searchResults)
                    {
                        if (!results.ContainsKey(searchResult.Document))
                            results.Add(searchResult.Document, new List<ISearchResult>());

                        results[searchString].Add(searchResult);
                    }
                }
            }
            finally
            {
                // Dispose all internal file streams.
                fileStreams?.Select(f => f.FileStream).ToList().ForEach(f => f.Dispose());
            }

            return Ok(results);
        }

        [HttpPost("advsearchdocs")]
        public async Task<IActionResult> AdvSearchDocs([FromForm] List<IFormFile> files, IEnumerable<string> searchString)
        {
            // ToDo: Similar to the above (maybe share some common methods such as loading), but performing
            // and advanced search ...

            await Task.CompletedTask;
            return Ok();
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
