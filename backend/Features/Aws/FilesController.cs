using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using backend.Features.Aws.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Aws
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _amazonS3;
        private string _bucketName;

        public FilesController(IConfiguration configuration, IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
            _bucketName = configuration.GetValue<string>("AWS:BucketName");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string? prefix)
        {
            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            await _amazonS3.PutObjectAsync(request);
            return Ok($"File {file.FileName} uploaded to S3 successfully");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFilesAsync()
        {
            var request = new ListObjectsV2Request()
            {
                BucketName = _bucketName,
                Prefix = null
            };

            var result = await _amazonS3.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = _bucketName,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddMinutes(10)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = _amazonS3.GetPreSignedURL(urlRequest)
                };
            });
            return Ok(s3Objects);
        }

        [HttpGet("preview")]
        public async Task<IActionResult> GetFileByKeyAsync(string key)
        {
            var s3Object = await _amazonS3.GetObjectAsync(_bucketName, key);
            return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFileAsync(string key)
        {
            await _amazonS3.DeleteObjectAsync(_bucketName, key);
            return Ok($"{key} deleted");
        }
    }
}
