namespace backend.Features.Projects.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Image? Image { get; set; }
        public string? Git { get; set; }
        public bool Active { get; set; } = true;
    }
}
