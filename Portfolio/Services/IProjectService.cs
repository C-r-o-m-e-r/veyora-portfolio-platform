using Portfolio.Models;

namespace Portfolio.Services;

public interface IProjectService
{
    IReadOnlyList<Project> GetAll();
    IReadOnlyList<Project> GetFeatured();
    Project? GetBySlug(string slug);
}
