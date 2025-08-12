using Microsoft.AspNetCore.Mvc;
using AppProject.Modules.FileExplorer;

namespace AppProject.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileExplorerController : ControllerBase
    {
        private readonly IFileExplorerService _fileExplorerService;

        public FileExplorerController(IFileExplorerService fileExplorerService)
        {
            _fileExplorerService = fileExplorerService;
        }

        [HttpGet("browse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchByQuery([FromQuery] string query, [FromQuery] string path = "")
        {
            var result = await _fileExplorerService.SearchByQuery(query, path);

            return result.Type switch
            {
                ResultType.Success => Ok(result.Data),
                ResultType.BadRequest => BadRequest(result.ErrorMessage),
                ResultType.Error => BadRequest(result.ErrorMessage),
                _ => BadRequest("Unknown error occurred")
            };
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<IActionResult> UploadFile([FromQuery] string path = "", IFormFile? file = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded");
            }

            using var stream = file.OpenReadStream();
            var result = await _fileExplorerService.UploadFile(path, stream, file.FileName);

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