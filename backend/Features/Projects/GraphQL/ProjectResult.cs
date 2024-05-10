using backend.Features.Projects.Models;

namespace backend.Features.Projects.GraphQL
{
    public class ProjectResult
    {
        public List<ProjectDto> Nodes { get; set; }
        public List<string> Errors { get; set; }
        public int Count { get; set; }
    }
}
