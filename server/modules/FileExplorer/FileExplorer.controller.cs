using Microsoft.AspNetCore.Mvc;
using AppProject.Modules.FileExplorer;
using AppProject.Modules.FileUpload;
using AppProject.Modules.FileSearch;

namespace AppProject.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileExplorerController : ControllerBase
    {
        private readonly IFileExplorerService _fileExplorerService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileSearchService _fileSearchService;

        public FileExplorerController(IFileExplorerService fileExplorerService, IFileUploadService fileUploadService, IFileSearchService fileSearchService)
        {
            _fileExplorerService = fileExplorerService;
            _fileUploadService = fileUploadService;
            _fileSearchService = fileSearchService;
        }

        [HttpGet("browse")]
        public async Task<IActionResult> BrowseDirectory([FromQuery] string path = "")
        {
            var result = await _fileExplorerService.BrowseDirectory(path);

            return result.Type switch
            {
                ResultType.Success => Ok(result.Data),
                ResultType.NotFound => NotFound(result.ErrorMessage),
                ResultType.Unauthorized => BadRequest(result.ErrorMessage),
                ResultType.Error => BadRequest(result.ErrorMessage),
                _ => BadRequest("Unknown error occurred")
            };
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("File path is required");
            }

            var result = await _fileExplorerService.GetFileForDownload(path);

            return result.Type switch
            {
                ResultType.Success => File(result.FileStream!, result.ContentType!, result.FileName!),
                ResultType.NotFound => NotFound(result.ErrorMessage),
                ResultType.Unauthorized => BadRequest(result.ErrorMessage),
                ResultType.Error => BadRequest(result.ErrorMessage),
                _ => BadRequest("Unknown error occurred")
            };
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByQuery([FromQuery] string query, [FromQuery] string path = "")
        {
            var result = await _fileSearchService.SearchFilesByQuery(query, path);

            return result.Type switch
            {
                ResultType.Success => Ok(result.Data),
                ResultType.BadRequest => BadRequest(result.ErrorMessage),
                ResultType.Error => BadRequest(result.ErrorMessage),
                _ => BadRequest("Unknown error occurred")
            };
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<IActionResult> UploadFile([FromQuery] string path = "", IFormFile? file = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded");
            }

            using var stream = file.OpenReadStream();
            var result = await _fileUploadService.UploadFile(path, stream, file.FileName);

            return result.Type switch
            {
                ResultType.Success => Ok(result.Data),
                ResultType.NotFound => NotFound(result.ErrorMessage),
                ResultType.Unauthorized => Unauthorized(result.ErrorMessage),
                ResultType.BadRequest => BadRequest(result.ErrorMessage),
                ResultType.Error => StatusCode(500, result.ErrorMessage),
                _ => BadRequest("Unknown error occurred")
            };
        }
    }
}