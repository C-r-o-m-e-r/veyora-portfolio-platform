using Portfolio.Models;

namespace Portfolio.Services;

public sealed class ProfileService : IProfileService
{
    private static readonly ProfileInfo Profile = new()
    {
        Name = "Orest Romaniuk",
        Headline = "Software Engineer | Full-Stack Developer",
        Summary = "I don't just write code — I design systems. IT student at WSB Merito Warsaw, passionate about software architecture, backend development, and building scalable full-stack applications. Currently looking for my first Junior or Internship role.",
        Education = "Computer Science Student at WSB Merito University (Warsaw, Poland).",
        Philosophy = "For me, the most exciting part of development happens before a single line of code is written. I love the architectural phase — designing databases, planning system interactions, and creating clean, scalable structures. I have solid experience building full-stack web apps (JS/Python), mobile backends (FastAPI), and games (Unity). Right now, I am actively diving deep into the .NET ecosystem.",
        SkillGroups =
        [
            new SkillGroup
            {
                Category = "Backend",
                Skills = ["Node.js", "Express.js", "Python (FastAPI)", "C# (.NET Core)"]
            },
            new SkillGroup
            {
                Category = "Frontend",
                Skills = ["JavaScript", "React", "Next.js", "HTML5", "CSS"]
            },
            new SkillGroup
            {
                Category = "Databases",
                Skills = ["PostgreSQL", "SQL"]
            },
            new SkillGroup
            {
                Category = "DevOps & Infrastructure",
                Skills = ["Linux", "Cloudflare", "Nginx", "Docker (learning)"]
            },
            new SkillGroup
            {
                Category = "Tools",
                Skills = ["Git", "GitHub", "Postman", "Jira", "Trello", "Matlab"]
            }
        ],
        HeroHighlights =
        [
            "C# (.NET Core)",
            "React / Next.js",
            "FastAPI",
            "PostgreSQL",
            "Linux",
            "Docker"
        ],
        Email = "orest@romaniuk.ch",
        GitHubUrl = "https://github.com/C-r-o-m-e-r",
        LinkedInUrl = "https://www.linkedin.com/in/orest-romaniuk/"
    };

    public ProfileInfo GetProfile() => Profile;
}
