using ApiCore.Common.Interfaces;
using DocParser.DocSearch;
using DocParser.ExtensionMethods;
using DocParser.Factories;
using DocParser.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCore.DocAnalysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocAnalysisController : ControllerBase
    {
        private IList<IDocParser> DocParsers = new List<IDocParser>();

        /// <summary>
        /// Gets the status of the API (returning active as default).
        /// </summary>
        /// <returns>Active status.</returns>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Api = "DocAnalysis",
                Status = "Active",
                Message = "Endpoint for document analysis API."
            });
        }

        /// <summary>
        /// Parses documents to extract hyperlinks within all the documents.
        /// </summary>
        /// <param name="files">Files to parse.</param>
        /// <returns>Parsed document links as JSON object.</returns>
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

        /// <summary>
        /// Searches multiple documents for a single search term, returning search results that indicate the
        /// document, paragraph, sentence, and position of each match found.
        /// </summary>
        /// <param name="files">Files to perform search on.</param>
        /// <param name="searchString">Search string.</param>
        /// <returns>Search results as JSON object.</returns>
        [HttpPost("searchdocs")]
        public async Task<IActionResult> SearchDocs([FromForm] List<IFormFile> files, [FromForm] string searchString)
        {
            var results = new Dictionary<string, IList<ISearchResult>>();
            IEnumerable<IFormFileStream>? fileStreams = null;

            try
            {
                // Convert form files to form file streams for the DocSearcher
                fileStreams = files.ToFormFileStreams();
                var searcher = await GetDocSearcher(fileStreams);

                // Assuming searcher has been initialised, perform the search and structure results into
                // a dictionary of file names and collective results to be returned.
                if (searcher != null)
                {
                    var searchResults = await searcher.Search(searchString);

                    foreach (var searchResult in searchResults)
                    {
                        if (!results.ContainsKey(searchResult.Document))
                            results.Add(searchResult.Document, new List<ISearchResult>());

                        results[searchResult.Document].Add(searchResult);
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

        /// <summary>
        /// Advanced doc search - looks for multiple terms within paragraphs within muliple documents, 
        /// returning match types and ratings depending on how close matches are over all terms within
        /// the search results.
        /// </summary>
        /// <param name="files">Files to perform search on.</param>
        /// <param name="searchStrings">Search strings.</param>
        /// <returns>Advanced search results as JSON object.</returns>
        [HttpPost("advsearchdocs")]
        public async Task<IActionResult> AdvSearchDocs([FromForm] List<IFormFile> files, 
            [FromForm] IEnumerable<string> searchStrings)
        {
            var results = new Dictionary<string, IList<IAdvancedSearchResult>>();
            IEnumerable<IFormFileStream>? fileStreams = null;

            try
            {
                fileStreams = files.ToFormFileStreams();
                var searcher = await GetDocSearcher(fileStreams);

                if (searcher != null)
                {
                    var searchResults = await searcher.AdvancedSearch(searchStrings.ToArray());

                    foreach (var searchResult in searchResults)
                    {
                        // Matches in paragraph should all relate to the same document. If this is null
                        // there is no data for this search result so continue to next (should not be the
                        // case as an advanced search result should always have at least 1 paragraph matched).
                        var document = searchResult.MatchesInParagraph.FirstOrDefault()?.Document;

                        if (document == null)
                            continue;

                        if (!results.ContainsKey(document))
                            results.Add(document, new List<IAdvancedSearchResult>());

                        results[document].Add(searchResult);
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

        /// <summary>
        /// Gets an initialised instance of <see cref="IDocSearcher"/> with input files loaded.
        /// </summary>
        /// <param name="ffStreams">Collection of <see cref="IFormFileStream"/> to load.</param>
        /// <returns>
        /// Initialised instance of <see cref="IDocSearcher"/>, or nul if initialisation timed out.
        /// </returns>
        private async Task<IDocSearcher?> GetDocSearcher(IEnumerable<IFormFileStream> ffStreams)
        {
            var searcher = new DocSearcher(ffStreams);

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

            if (searcher?.IsInitialised ?? false)
                return searcher;

            return null;
        }

        /// <summary>
        /// Gets a unique file name should the filename already exist in collection of files.
        /// </summary>
        /// <param name="filename">File name to check.</param>
        /// <param name="existingFileNames">Existing files collection.</param>
        /// <returns>
        /// Unique file name for the file (if not already unique), where original filename will be used 
        /// and appended by number.
        /// </returns>
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
