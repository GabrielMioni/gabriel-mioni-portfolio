using System.Text.RegularExpressions;

namespace Portfolio.Api.Domain.Projects;

public class ProjectTag
{
    public const int MaxNameLength = 50;
    public const int MaxValueLength = 50;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Value { get; private set; } = default!;
    public ICollection<Project> Projects { get; private set; } = new List<Project>();

    private ProjectTag() { } // EF

    public static ProjectTag Create(string name)
    {
        var (normalizedName, value) = NormalizeAndValidate(name);

        return new ProjectTag
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Value = value
        };
    }

    public void Rename(string name)
    {
        var (normalizedName, value) = NormalizeAndValidate(name);

        Name = normalizedName;
        Value = value;
    }

    public static string GenerateValue(string name)
    {
        var trimmed = name.Trim().ToLowerInvariant();
        var hyphenated = Regex.Replace(trimmed, @"[\s\-]+", "-");
        return Regex.Replace(hyphenated, @"-+", "-").Trim('-');
    }

    private static (string Name, string Value) NormalizeAndValidate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(name));

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Tag name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }

        var value = GenerateValue(normalizedName);

        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Tag name must produce a usable value.", nameof(name));

        if (value.Length > MaxValueLength)
        {
            throw new ArgumentException(
                $"Tag value cannot exceed {MaxValueLength} characters.",
                nameof(name));
        }

        return (normalizedName, value);
    }
}
