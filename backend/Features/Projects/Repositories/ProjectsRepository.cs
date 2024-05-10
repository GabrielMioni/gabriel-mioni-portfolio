using backend.Data;
using backend.Features.Aws.Services;
using backend.Features.Projects.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace backend.Features.Projects.Repositories
{
    public class ProjectsRepository : IProjectsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAwsService _awsService;
        public ProjectsRepository(ApplicationDbContext context, IAwsService awsService)
        {
            _context = context;
            _awsService = awsService;
        }

        public async Task<List<ProjectDto>> GetProjectsAsync(int skip = 0, int take = 10, bool includeInactive = false)
        {
            var query = _context.Projects.AsQueryable();
            query = query.Include(p => p.Image);

            if (!includeInactive)
            {
                query = query.Where(p => p.Active);
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            var projects = await query.Skip(skip).Take(take).ToListAsync();
            var projectDtos = new List<ProjectDto>();
            foreach (var project in projects)
            {
                var fileName = project.Image?.FileName;
                var fileUrl = fileName != null ? _awsService.GetFileUrlByKey(fileName) : null;

                projectDtos.Add(new ProjectDto()
                {
                    Id = project.Id,
                    Active = project.Active,
                    Description = project.Description,
                    Git = project.Git,
                    ImageUrl = fileUrl,
                    Name = project.Name
                });
            }

            return projectDtos;
        }

        public async Task<Project> AddProjectAsync(Project newProject)
        {
            newProject.Id = Guid.NewGuid().ToString();
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            return newProject;
        }

        public async Task<OperationResult> SetProjectActiveAsync(string id, Boolean setActive)
        {
            var project = _context.Projects.Where(p => p.Id  == id).FirstOrDefault();

            var result = new OperationResult();

            if (project == null)
            {
                return OperationResult.Fail($"Unable to find project with id {id} of ");
            }

            project.Active = setActive;
            await _context.SaveChangesAsync();

            var action = setActive ? "activated" : "removed";

            return OperationResult.Success($"Project with id {id} has been successfully {action}.");
        }

        public async Task<OperationResult> EditProjectAsync(string id, string name, string description, string? git = "")
        {
            var project = _context.Projects.Where(p => p.Id == id).FirstOrDefault();

            var result = new OperationResult();

            if (project == null) {
                return OperationResult.Fail($"Unable to find project with id {id} of ");
            }

            project.Name = name;
            project.Git = git;
            project.Description = description;

            await _context.SaveChangesAsync();

            return OperationResult.Success($"Project with id {id} has been updated.");
        }

        public async Task<int> GetProjectCountAsync()
        {
            return await _context.Projects.CountAsync();
        }
    }
}
