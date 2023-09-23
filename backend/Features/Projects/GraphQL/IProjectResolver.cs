using backend.Features.Projects.Models;

namespace backend.Features.Projects.GraphQL
{
    public interface IProjectResolver
    {
        public Task<IEnumerable<Project>> GetAllProjectsAsync();
    }
}
