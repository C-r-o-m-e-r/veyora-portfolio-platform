namespace Portfolio.Models;

public sealed class ProfileInfo
{
    public required string Name { get; init; }
    public required string Headline { get; init; }
    public required string Summary { get; init; }
    public required string Education { get; init; }
    public required string Philosophy { get; init; }
    public required IReadOnlyList<SkillGroup> SkillGroups { get; init; }
    public required IReadOnlyList<string> HeroHighlights { get; init; }
    public required string Email { get; init; }
    public string? GitHubUrl { get; init; }
    public string? LinkedInUrl { get; init; }
}
