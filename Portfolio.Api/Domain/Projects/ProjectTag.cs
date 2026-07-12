using System.Text.RegularExpressions;

namespace Portfolio.Api.Domain.Projects;

public class ProjectTag
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Value { get; private set; } = default!;
    public ICollection<Project> Projects { get; private set; } = new List<Project>();

    private ProjectTag() { } // EF

    public static ProjectTag Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(name));

        var trimmed = name.Trim();

        return new ProjectTag
        {
            Id = Guid.NewGuid(),
            Name = trimmed,
            Value = GenerateValue(trimmed)
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(name));

        var trimmed = name.Trim();
        Name = trimmed;
        Value = GenerateValue(trimmed);
    }

    public static string GenerateValue(string name)
    {
        var trimmed = name.Trim().ToLowerInvariant();
        var hyphenated = Regex.Replace(trimmed, @"[\s\-]+", "-");
        return Regex.Replace(hyphenated, @"-+", "-").Trim('-');
    }
}
