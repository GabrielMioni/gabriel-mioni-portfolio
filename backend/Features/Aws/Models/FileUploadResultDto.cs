namespace backend.Features.Aws.Models
{
    public class FileUploadResultDto
    {
        public string S3Key { get; set; }
        public string DbRecordId { get; set; }
    }
}
