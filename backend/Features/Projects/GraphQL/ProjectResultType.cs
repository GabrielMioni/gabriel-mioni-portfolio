namespace backend.Features.Projects.GraphQL
{
    public class ProjectResultType : ObjectType<ProjectResult>
    {
        protected override void Configure(IObjectTypeDescriptor<ProjectResult> descriptor)
        {
            descriptor.Description("Represents the result of a project query");

            descriptor
                .Field(p => p.Nodes)
                .Description("Projects")
                .Type<ProjectType>();

            descriptor
                .Field(p => p.Errors)
                .Description("Any errors that occurred during the query")
                .Type<ListType<NonNullType<StringType>>>();
        }
    }
}
