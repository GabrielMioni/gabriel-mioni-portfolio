using backend.Features.Aws.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Aws
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private IAwsService _awsService;

        public FilesController(IAwsService awsService)
        {
            _awsService = awsService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string? prefix)
        {
            await _awsService.UploadFileAsync(file, prefix);
            return Ok($"File {file.FileName} uploaded to S3 successfully");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFilesAsync()
        {
            var s3Objects = await _awsService.GetAllFilesAsync();
            return Ok(s3Objects);
        }

        [HttpGet("preview")]
        public async Task<IActionResult> GetFileByKeyAsync(string key)
        {
            var fileResult = await _awsService.GetFileByKeyAsync(key);
            return File(fileResult.FileStream, fileResult.ContentType);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFileAsync(string key)
        {
            await _awsService.DeleteFileByKeyAsync(key);
            return Ok($"{key} deleted");
        }
    }
}
