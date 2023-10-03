using backend.Features.Projects.Models;

namespace backend.Features.Projects.GraphQL
{
    public class ProjectResultType : ObjectType<ProjectResult>
    {
        protected override void Configure(IObjectTypeDescriptor<ProjectResult> descriptor)
        {
            descriptor.Description("Represents the result of a project query");

            descriptor
                .Field(p => p.Projects)
                .Description("Projects")
                .Type<Project>();

            descriptor
                .Field(p => p.Errors)
                .Description("Any errors that occurred during the query")
                .Type<ListType<NonNullType<StringType>>>();
        }
    }
}
