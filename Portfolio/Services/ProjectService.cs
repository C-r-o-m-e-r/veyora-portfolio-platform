using Portfolio.Models;

namespace Portfolio.Services;

public sealed class ProjectService : IProjectService
{
    private static readonly IReadOnlyList<Project> Projects =
    [
        new()
        {
            Slug = "ecommerce-marketplace",
            Title = "Full-Stack E-Commerce Marketplace",
            ShortDescription = "A comprehensive monorepo e-commerce platform with a React frontend, Node.js API, and secure Stripe payment integration.",
            Problem = "Building a scalable, secure platform where multiple sellers can manage products and buyers can securely checkout.",
            Solution = "Engineered a robust decoupled architecture using a Vite-React frontend and a Node.js/Express backend. Implemented a complete CI/CD pipeline using GitHub Actions for automated testing (over 50% backend coverage via Jest) and linting.",
            Result = "Delivered a containerized monorepo marketplace with automated CI/CD, role-based access control, and production-grade Stripe payment flows.",
            KeyFeatures =
            [
                "JWT-based Role Access Control (Buyer / Seller / Admin)",
                "Dynamic coupon system with server-side validation",
                "Server-side cart merging for authenticated and guest sessions",
                "Hierarchical category filtering across the product catalog",
                "Fully integrated Stripe webhooks for payment processing",
                "Backend containerized via Docker for seamless deployment"
            ],
            TechStack =
            [
                "React",
                "Node.js",
                "Express",
                "PostgreSQL",
                "Prisma ORM",
                "Stripe API",
                "Docker",
                "CI/CD (GitHub Actions)",
                "Jest / Cypress"
            ],
            Featured = true
        },
        new()
        {
            Slug = "smart-calorie-tracker",
            Title = "Smart Calorie Tracker AI (Academic Prototype)",
            ShortDescription = "A decoupled mobile ecosystem utilizing an asynchronous Python backend and Generative AI Vision to analyze food macros from photos.",
            Problem = "Traditional calorie trackers require tedious manual input. The goal was to automate macro-tracking using state-of-the-art AI while ensuring a fast, non-blocking backend.",
            Solution = "Designed a high-performance backend using FastAPI and asynchronous PostgreSQL (asyncpg). The iOS client was built with SwiftUI strictly following the MVVM pattern for a reactive, stateless UI.",
            Result = "Shipped an async FastAPI backend with AI-powered food analysis, secure OAuth2 token rotation, and Alembic-managed schemas — powering a reactive SwiftUI iOS client.",
            KeyFeatures =
            [
                "GPT Vision integration to estimate calories, proteins, fats, and carbs from uploaded images (Base64 encoding)",
                "Secure OAuth2 flow with JWT access / refresh token rotation",
                "Rate-limiting via SlowAPI to protect expensive AI endpoints",
                "Database schemas strictly managed via Alembic migrations",
                "Decoupled architecture: SwiftUI MVVM client consuming a stateless async API"
            ],
            TechStack =
            [
                "Python",
                "FastAPI",
                "PostgreSQL",
                "SQLAlchemy",
                "GPT Vision AI",
                "Swift (iOS)",
                "Docker"
            ],
            Featured = true
        },
        new()
        {
            Slug = "logic-puzzle-games",
            Title = "Logic Puzzle Games",
            ShortDescription = "Developed a series of logic puzzle games focusing on game mechanics, state management, and algorithmic problem-solving.",
            Problem = "Each puzzle needed distinct mechanics while sharing a consistent game loop, state transitions, and win/lose logic without duplicated code across titles.",
            Solution = "Built multiple games in Unity with C#, applying reusable state machines, component-based design, and algorithmic solvers for level validation and hint generation.",
            Result = "Created a portfolio of playable puzzle games demonstrating strong fundamentals in game architecture, C# patterns, and problem decomposition.",
            KeyFeatures = [],
            TechStack = ["Unity", "C#"],
            Featured = false
        },
        new()
        {
            Slug = "personal-cloud-infrastructure",
            Title = "Personal Cloud Infrastructure & Portfolio",
            ShortDescription = "Designed and deployed my own portfolio using modern .NET technologies, hosted on a self-managed Linux server with reverse proxy and edge security.",
            Problem = "Needed a professional, self-hosted presence with HTTPS, edge protection, and full control over the deployment stack — without relying on managed PaaS platforms.",
            Solution = "Built this portfolio with .NET 8 Blazor on Ubuntu, fronted by Nginx as a reverse proxy. Secured the domain with Cloudflare edge protection and Certbot-managed TLS certificates.",
            Result = "Live, recruiter-ready portfolio running on infrastructure I designed and maintain — from DNS and SSL to server configuration and application deployment.",
            KeyFeatures = [],
            TechStack = ["C# (.NET 8 Blazor)", "Linux (Ubuntu)", "Nginx", "Cloudflare", "Certbot"],
            Featured = false
        }
    ];

    public IReadOnlyList<Project> GetAll() => Projects;

    public IReadOnlyList<Project> GetFeatured() =>
        Projects.Where(p => p.Featured).ToList();

    public Project? GetBySlug(string slug) =>
        Projects.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
}
