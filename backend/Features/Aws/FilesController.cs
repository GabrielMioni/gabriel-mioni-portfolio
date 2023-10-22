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
        private readonly IConfiguration _configuration;
        public FilesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string? prefix)
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var regionString = _configuration.GetValue<string>("AWS:Region");

            var region = RegionEndpoint.GetBySystemName(regionString);

            var request = new PutObjectRequest()
            {
                BucketName = "portfolio-gjam-dev",
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            var s3Client = new AmazonS3Client(accessKey, secretKey, region);
            await s3Client.PutObjectAsync(request);
            return Ok($"File {file.FileName} uploaded to S3 successfully");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFilesAsync()
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var regionString = _configuration.GetValue<string>("AWS:Region");

            var region = RegionEndpoint.GetBySystemName(regionString);

            var s3Client = new AmazonS3Client(accessKey, secretKey, region);
            var request = new ListObjectsV2Request()
            {
                BucketName = "portfolio-gjam-dev",
                Prefix = null
            };

            var result = await s3Client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = "portfolio-gjam-dev",
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddMinutes(10)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = s3Client.GetPreSignedURL(urlRequest)
                };
            });
            return Ok(s3Objects);
        }
    }
}
