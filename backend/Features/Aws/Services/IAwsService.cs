using Amazon.S3.Model;
using backend.Features.Aws.Models;

namespace backend.Features.Aws.Services
{
    public interface IAwsService
    {
        public Task<FileUploadResultDto> UploadFileAsync(IFormFile file, string key);
        public Task<IEnumerable<S3ObjectDto>> GetAllFilesAsync();
        public Task<FileResultDto> GetFileByKeyAsync(string key);
        public string GetFileUrlByKey(string key);
        public Task DeleteFileByKeyAsync(string key);
    }
}
