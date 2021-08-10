using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PDFDocMan.Api.Db;

namespace PDFDocMan.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocsController : ControllerBase
    {
        private readonly ILogger<DocsController> _logger;
        private readonly DocDbContext _context;
        private readonly DocRepo _docRepo;

        public DocsController(ILogger<DocsController> logger, DocDbContext context)
        {
            _logger = logger;
            _context = context;
            _docRepo = new DocRepo(context);
        }

        private string? _basePath;
        private string BasePath
        {
            get
            {
                if (_basePath == null)
                    _basePath = Url.ActionLink("GetList");
                return _basePath;
            }
        }

        private string ControllerName => ControllerContext.ActionDescriptor.ControllerName;
        private string ActionName => ControllerContext.ActionDescriptor.ActionName;

        private void LogInfo(string message) => _logger.LogInformation($"{ControllerName}.{ActionName}: {message}");
        private void LogWarning(string message) => _logger.LogWarning($"{ControllerName}.{ActionName}: {message}");

        /// <summary>
        /// Gets the full list of documents.
        /// </summary>
        /// <returns>A list of zero or more documents.</returns>
        [HttpGet(Name = "GetList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<ApiDoc>> GetList()
        {
            var adocs = (await _docRepo.GetAll()).GetApiDocs(BasePath);
            LogInfo($"returning {adocs.Count()} documents");
            return adocs;
        }

        /// <summary>
        /// Gets the PDF for the document with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The file stream.</returns>
        [HttpGet("{id}", Name = "Get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            const string mimeType = "application/pdf";

            try
            {
                var doc = await _docRepo.GetById(id);
                LogInfo($"returning document {id} content");
                var s = new MemoryStream(doc.Binary);
                return new FileStreamResult(s, mimeType);
            }
            catch (NotFoundException nfx)
            {
                LogInfo($"returning not found response: {nfx.Message}");
                return NotFound(nfx.Message);
            }
        }

        /// <summary>
        /// Creates a document from the given file, which should be a PDF.
        /// </summary>
        /// <param name="file">The IFormFile object provided by the multipart form upload.</param>
        /// <returns>CretedAtRoute/BadRequest</returns>
        [HttpPost(Name = "Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(IFormFile file)
        {
            try
            {
                var doc = await file.GetPDFDoc();
                await _docRepo.Create(doc);
                LogInfo($"document id {doc.Id} created");
                var x = CreatedAtRoute("Get", new { id = doc.Id }, doc.GetApiDoc(BasePath));
                return CreatedAtRoute("Get", new { doc.Id }, doc.GetApiDoc(BasePath));
            }
            catch (ValidationException vex)
            {
                LogWarning($"returning bad request response {vex.Message}");
                return BadRequest(vex.Message);
            }
        }

        /// <summary>
        /// Deletes the given document
        /// </summary>
        /// <param name="id"></param>
        /// <returns>NoContent/NotFound</returns>
        [HttpDelete("{id}", Name = "Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _docRepo.Delete(id);
                LogInfo($"deleted document {id}");
                return NoContent();
            }
            catch (NotFoundException nfx)
            {
                LogInfo($"returning not found response: {nfx.Message}");
                return NotFound(nfx.Message);
            }
        }

        /// <summary>
        /// Reorders the documents based on the given order of the ids in the array.
        /// </summary>
        /// <param name="ids">The list of all document ids in the chosen order.</param>
        /// <returns>OK (with reordered docs list)/NotFound/BadRequest</returns>
        [HttpPut("reorder", Name = "Reorder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Reorder(int[] ids)
        {
            try
            {
                var adocs = (await _docRepo.ReOrder(ids)).GetApiDocs(BasePath);
                return Ok(adocs);
            }
            catch (NotFoundException nfx)
            {
                LogInfo($"returning not found response: {nfx.Message}");
                return NotFound(nfx.Message);
            }
            catch (ValidationException vex)
            {
                LogWarning($"returning bad request response: {vex.Message}");
                return BadRequest(vex.Message);
            }
        }
    }
}
