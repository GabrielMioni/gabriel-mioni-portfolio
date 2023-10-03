﻿using backend.Features.Projects.Models;

namespace backend.Features.Projects.GraphQL
{
    public class ProjectResult
    {
        public List<Models.Project> Projects { get; set; }
        public List<string> Errors { get; set; }
    }
}