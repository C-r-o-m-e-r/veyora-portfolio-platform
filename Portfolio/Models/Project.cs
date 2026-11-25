namespace Portfolio.Models;

public sealed class Project
{
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string ShortDescription { get; init; }
    public required string Problem { get; init; }
    public required string Solution { get; init; }
    public string? Result { get; init; }
    public required IReadOnlyList<string> KeyFeatures { get; init; }
    public required IReadOnlyList<string> TechStack { get; init; }
    public string? GitHubUrl { get; init; }
    public string? LiveUrl { get; init; }
    public bool Featured { get; init; }

    public bool HasDeepDive => KeyFeatures.Count > 0;
}
