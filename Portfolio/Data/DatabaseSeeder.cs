using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var environment = services.GetRequiredService<IHostEnvironment>();
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);

        var db = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<string>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await db.Database.EnsureCreatedAsync();

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AdminUsers" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AdminUsers" PRIMARY KEY AUTOINCREMENT,
                "Username" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_AdminUsers_Username" ON "AdminUsers" ("Username");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ContactMessages" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ContactMessages" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Email" TEXT NOT NULL,
                "Message" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_ContactMessages_CreatedAt" ON "ContactMessages" ("CreatedAt");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AvailableSlots" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AvailableSlots" PRIMARY KEY AUTOINCREMENT,
                "Date" TEXT NOT NULL,
                "StartTime" TEXT NOT NULL,
                "IsBooked" INTEGER NOT NULL DEFAULT 0
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_AvailableSlots_Date_StartTime" ON "AvailableSlots" ("Date", "StartTime");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_AvailableSlots_IsBooked" ON "AvailableSlots" ("IsBooked");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "BookingRequests" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_BookingRequests" PRIMARY KEY AUTOINCREMENT,
                "GuestName" TEXT NOT NULL,
                "GuestEmail" TEXT NOT NULL,
                "Reason" TEXT NOT NULL,
                "RequestedDateTime" TEXT NOT NULL,
                "Status" INTEGER NOT NULL,
                "AvailableSlotId" INTEGER NULL
            );
            """);

        await EnsureBookingSlotColumnAsync(db);
        await EnsureTodoItemColumnsAsync(db);
        await EnsureUserCredentialsTableAsync(db);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_BookingRequests_RequestedDateTime" ON "BookingRequests" ("RequestedDateTime");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_BookingRequests_Status" ON "BookingRequests" ("Status");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_BookingRequests_AvailableSlotId" ON "BookingRequests" ("AvailableSlotId");
            """);

        if (await db.AdminUsers.AnyAsync())
        {
            return;
        }

        var adminUsername = configuration["Admin:Username"];
        var adminPassword = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(adminUsername) && string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Both Admin:Username and Admin:Password must be configured to seed the initial admin account.");
        }

        db.AdminUsers.Add(new AdminUser
        {
            Username = adminUsername,
            PasswordHash = passwordHasher.HashPassword(adminUsername, adminPassword)
        });

        await db.SaveChangesAsync();
    }

    private static async Task EnsureBookingSlotColumnAsync(AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(\"BookingRequests\")";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader.GetString(1), "AvailableSlotId", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
        }
        finally
        {
            await connection.CloseAsync();
        }

        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "BookingRequests" ADD COLUMN "AvailableSlotId" INTEGER NULL;
            """);
    }

    private static async Task EnsureTodoItemColumnsAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "TodoItems" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_TodoItems" PRIMARY KEY AUTOINCREMENT,
                "Title" TEXT NOT NULL,
                "Description" TEXT NOT NULL DEFAULT '',
                "StartDate" TEXT NOT NULL,
                "Deadline" TEXT NOT NULL,
                "IsCompleted" INTEGER NOT NULL DEFAULT 0
            );
            """);

        await EnsureTodoColumnAsync(db, "Description", """
            ALTER TABLE "TodoItems" ADD COLUMN "Description" TEXT NOT NULL DEFAULT '';
            """);

        await EnsureTodoColumnAsync(db, "StartDate", """
            ALTER TABLE "TodoItems" ADD COLUMN "StartDate" TEXT NULL;
            """);

        await EnsureTodoColumnAsync(db, "Deadline", """
            ALTER TABLE "TodoItems" ADD COLUMN "Deadline" TEXT NULL;
            """);

        await EnsureTodoColumnAsync(db, "Priority", """
            ALTER TABLE "TodoItems" ADD COLUMN "Priority" INTEGER NOT NULL DEFAULT 0;
            """);

        await db.Database.ExecuteSqlRawAsync("""
            UPDATE "TodoItems"
            SET "StartDate" = COALESCE("StartDate", "Date", datetime('now'))
            WHERE "StartDate" IS NULL;
            """);

        await db.Database.ExecuteSqlRawAsync("""
            UPDATE "TodoItems"
            SET "Deadline" = COALESCE(
                CASE
                    WHEN "Deadline" IS NOT NULL AND length("Deadline") <= 8 THEN datetime("Date" || 'T' || "Deadline")
                    ELSE "Deadline"
                END,
                datetime(COALESCE("Date", date('now')) || 'T23:59:59')
            )
            WHERE "Deadline" IS NULL OR length("Deadline") <= 8;
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_TodoItems_StartDate" ON "TodoItems" ("StartDate");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_TodoItems_Deadline" ON "TodoItems" ("Deadline");
            """);
    }

    private static async Task EnsureUserCredentialsTableAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "UserCredentials" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_UserCredentials" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "CredentialId" BLOB NOT NULL,
                "PublicKey" BLOB NOT NULL,
                "UserHandle" BLOB NOT NULL,
                "SignatureCounter" INTEGER NOT NULL DEFAULT 0,
                "CredType" TEXT NOT NULL DEFAULT 'public-key',
                "RegDate" TEXT NOT NULL,
                CONSTRAINT "FK_UserCredentials_AdminUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AdminUsers" ("Id") ON DELETE CASCADE
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_UserCredentials_UserId" ON "UserCredentials" ("UserId");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserCredentials_CredentialId" ON "UserCredentials" ("CredentialId");
            """);
    }

    private static async Task EnsureTodoColumnAsync(AppDbContext db, string columnName, string alterSql)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(\"TodoItems\")";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
        }
        finally
        {
            await connection.CloseAsync();
        }

        await db.Database.ExecuteSqlRawAsync(alterSql);
    }
}
