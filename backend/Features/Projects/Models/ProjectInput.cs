using System.ComponentModel.DataAnnotations;

namespace backend.Features.Projects.Models
{
    [InputObjectType]
    public class ProjectInput
    {
        [GraphQLNonNullType]
        [Required]
        public string Name { get; set; }

        [GraphQLNonNullType]
        [Required]
        public string Description { get; set; }
        public string? Git { get; set; }
    }
}
