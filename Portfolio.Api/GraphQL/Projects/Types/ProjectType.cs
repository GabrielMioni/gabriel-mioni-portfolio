using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.GraphQL.Projects.Types
{
    public class ProjectType: ObjectType<Project>
    {
        protected override void Configure(IObjectTypeDescriptor<Project> descriptor)
        {
            descriptor.Name("Project");

            descriptor.Field(p => p.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(p => p.Title).Type<NonNullType<StringType>>();
            descriptor.Field(p => p.Summary);
            descriptor.Field(p => p.Body);

            descriptor.Field(p => p.Status);

            descriptor.Field(p => p.CreatedAt);
            descriptor.Field(p => p.PublishedAt);
            descriptor.Field(p => p.PublishedAt);
        }
    }
}
