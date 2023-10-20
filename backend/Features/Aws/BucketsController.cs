using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Aws
{
    [Route("api/[controller]")]
    public class BucketsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public BucketsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListAsync()
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var regionString = _configuration.GetValue<string>("AWS:Region");

            var region = RegionEndpoint.GetBySystemName(regionString);

            // var s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USWest2);
            var s3Client = new AmazonS3Client(accessKey, secretKey, region);
            var data = await s3Client.ListBucketsAsync();
            var buckets = data.Buckets.Select(b => { return b.BucketName; });
            return Ok(buckets);
        }
    }
}
