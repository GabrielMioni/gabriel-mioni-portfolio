﻿using backend.Features.Projects.Models;

namespace backend.Features.Projects.Repositories
{
    public interface IProjectsRepository
    {
        Task<Project> AddProjectAsync(Project newProject);
        Task<List<Project>> GetProjectsAsync(int skip, int take);
        Task<int> GetProjectCountAsync();
    }
}
