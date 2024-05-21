using backend.Features.Aws.Services;
using backend.Features.Projects.Models;
using backend.Features.Projects.Repositories;
using Microsoft.AspNetCore.Mvc;
using Path = System.IO.Path;

namespace backend.Features.Aws
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private IAwsService _awsService;
        private IImageRepository _imageRepository;

        public FilesController(IAwsService awsService, IImageRepository imageRepository)
        {
            _awsService = awsService;
            _imageRepository = imageRepository;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string? prefix)
        {
            await _awsService.UploadFileAsync(file, prefix);
            return Ok($"File {file.FileName} uploaded to S3 successfully");
        }

        [HttpPost("project/{projectId}")]
        public async Task<IActionResult> UploadProjectImageAsync(IFormFile file, string projectId)
        {
            try
            {
                var uniqueFileName = Guid.NewGuid().ToString();

                var image = new Image()
                {
                    AltText = file.FileName,
                    FileName = uniqueFileName,
                    Title = file.FileName,
                    ContentType = file.ContentType, 
                    Size = file.Length,
                    ProjectId = projectId
                };
                await _imageRepository.AddImageAsync(image);
                await _awsService.UploadFileAsync(file, uniqueFileName);

                var fileUrl = _awsService.GetFileUrlByKey(uniqueFileName);

                return Ok(new { message = $"File {file.FileName} uploaded to S3 successfully", fileUrl });

            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
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
