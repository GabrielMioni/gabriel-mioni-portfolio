using Amazon.S3;
using Amazon.S3.Model;
using backend.Features.Aws.Models;

namespace backend.Features.Aws.Services
{
    public class AwsService : IAwsService
    {
        private readonly IAmazonS3 _amazonS3;
        private string _bucketName;

        public AwsService(IConfiguration configuration, IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
            _bucketName = configuration.GetValue<string>("AWS:BucketName");
        }

        public async Task DeleteFileByKeyAsync(string key)
        {
            await _amazonS3.DeleteObjectAsync(_bucketName, key);
        }

        public async Task<IEnumerable<S3ObjectDto>> GetAllFilesAsync()
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

            return s3Objects;
        }

        public async Task<FileResultDto> GetFileByKeyAsync(string key)
        {
            var s3Object = await _amazonS3.GetObjectAsync(_bucketName, key);
            var fileResult = new FileResultDto()
            {
                FileStream = s3Object.ResponseStream,
                ContentType = s3Object.Headers.ContentType
            };

            return fileResult;
        }

        public async Task<FileUploadResultDto> UploadFileAsync(IFormFile file, string? prefix)
        {
            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            await _amazonS3.PutObjectAsync(request);

            var fileUploadResult = new FileUploadResultDto()
            {
                S3Key = file.FileName,
                DbRecordId = "1"
            };

            return fileUploadResult;
        }
    }
}
