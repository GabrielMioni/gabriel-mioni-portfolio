using backend.Features.Aws.Services;

namespace backend.Features.Projects.Models
{
    public class ProjectDto
    {        
        public string Id { get; set; }
        public bool Active { get; set; }
        public string Description { get; set; }
        public string? Git { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; }
    }
}
