namespace Portfolio.Api.Domain.Projects
{
    public class Project
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Summary { get; set; }
        public string? Body { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public ProjectStatus? Status { get; set; }
    }
}
