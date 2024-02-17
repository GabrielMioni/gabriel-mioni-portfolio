namespace backend.Features.Projects.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string AltText { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public long Size { get; set; }

        // Navigational properties
        public string ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
