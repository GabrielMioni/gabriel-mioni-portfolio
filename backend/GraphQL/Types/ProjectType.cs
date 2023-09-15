using backend.Models;

namespace backend.GraphQL.Types
{
    public class ProjectType : ObjectType<Project>
    {
        protected override void Configure(IObjectTypeDescriptor<Project> descriptor)
        {
            descriptor.Description("Represents a project");

            descriptor
                .Field(p => p.Id)
                .Description("Unique identifier for the project");

            descriptor
                .Field(p => p.Name)
                .Description("Name of the project");

            descriptor
                .Field(p => p.Description)
                .Description("Description of the project");

            descriptor
                .Field(p => p.Image)
                .Description("Image for the project");

            descriptor
                .Field(p => p.Image)
                .Description("Image for the project");
        }
    }
}
