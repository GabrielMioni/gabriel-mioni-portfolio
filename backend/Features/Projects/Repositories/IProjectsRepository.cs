﻿using backend.Features.Projects.Models;

namespace backend.Features.Projects.Repositories
{
    public interface IProjectsRepository
    {
        Task<Project> AddProjectAsync(Project newProject);
        Task<List<ProjectDto>> GetProjectsAsync(int skip, int take, bool IncludeInactive = false);
        Task<int> GetProjectCountAsync();
        Task<OperationResult> SetProjectActiveAsync(string id, Boolean value);
        Task<OperationResult> EditProjectAsync(string id, string name, string description, string? git = "");
    }
}
