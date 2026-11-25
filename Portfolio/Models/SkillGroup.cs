namespace Portfolio.Models;

public sealed class SkillGroup
{
    public required string Category { get; init; }
    public required IReadOnlyList<string> Skills { get; init; }
}
