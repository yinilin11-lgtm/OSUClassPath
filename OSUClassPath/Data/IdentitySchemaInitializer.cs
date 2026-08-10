using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OSUClassPath.Data;

#pragma warning disable EF1002
public static class IdentitySchemaInitializer
{
    public static async Task EnsureAsync(AdvisorDbContext dbContext)
    {
        await AddColumnIfMissingAsync(dbContext, "StudentCourses", "UserId", "TEXT");
        await AddColumnIfMissingAsync(dbContext, "StudentCourses", "StudentId", "INTEGER");
        await AddColumnIfMissingAsync(dbContext, "Courses", "Category", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(dbContext, "Courses", "Track", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync(dbContext, "AspNetUsers", "AcademicYear", "INTEGER NOT NULL DEFAULT 1");

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AspNetRoles" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
                "Name" TEXT NULL,
                "NormalizedName" TEXT NULL,
                "ConcurrencyStamp" TEXT NULL
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AspNetUsers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
                "DisplayName" TEXT NOT NULL DEFAULT '',
                "Program" TEXT NOT NULL DEFAULT 'BS CSE',
                "CatalogYear" INTEGER NOT NULL DEFAULT 2026,
                "AcademicYear" INTEGER NOT NULL DEFAULT 1,
                "StartingTerm" TEXT NOT NULL DEFAULT 'Autumn',
                "PreferredCredits" INTEGER NOT NULL DEFAULT 15,
                "UserName" TEXT NULL,
                "NormalizedUserName" TEXT NULL,
                "Email" TEXT NULL,
                "NormalizedEmail" TEXT NULL,
                "EmailConfirmed" INTEGER NOT NULL DEFAULT 0,
                "PasswordHash" TEXT NULL,
                "SecurityStamp" TEXT NULL,
                "ConcurrencyStamp" TEXT NULL,
                "PhoneNumber" TEXT NULL,
                "PhoneNumberConfirmed" INTEGER NOT NULL DEFAULT 0,
                "TwoFactorEnabled" INTEGER NOT NULL DEFAULT 0,
                "LockoutEnd" TEXT NULL,
                "LockoutEnabled" INTEGER NOT NULL DEFAULT 0,
                "AccessFailedCount" INTEGER NOT NULL DEFAULT 0
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
                "RoleId" TEXT NOT NULL,
                "ClaimType" TEXT NULL,
                "ClaimValue" TEXT NULL,
                CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
                "UserId" TEXT NOT NULL,
                "ClaimType" TEXT NULL,
                "ClaimValue" TEXT NULL,
                CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
                "LoginProvider" TEXT NOT NULL,
                "ProviderKey" TEXT NOT NULL,
                "ProviderDisplayName" TEXT NULL,
                "UserId" TEXT NOT NULL,
                CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
                CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
                "UserId" TEXT NOT NULL,
                "RoleId" TEXT NOT NULL,
                CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
                CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
                "UserId" TEXT NOT NULL,
                "LoginProvider" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Value" TEXT NULL,
                CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
                CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ChatSessions" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ChatSessions" PRIMARY KEY AUTOINCREMENT,
                "UserId" TEXT NOT NULL,
                "Title" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL,
                CONSTRAINT "FK_ChatSessions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ChatMessages" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ChatMessages" PRIMARY KEY AUTOINCREMENT,
                "ChatSessionId" INTEGER NOT NULL,
                "Role" TEXT NOT NULL,
                "Content" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                CONSTRAINT "FK_ChatMessages_ChatSessions_ChatSessionId" FOREIGN KEY ("ChatSessionId") REFERENCES "ChatSessions" ("Id") ON DELETE CASCADE
            );
            """);

        await CreateIndexIfMissingAsync(dbContext, "RoleNameIndex", "AspNetRoles", "NormalizedName", unique: true);
        await CreateIndexIfMissingAsync(dbContext, "EmailIndex", "AspNetUsers", "NormalizedEmail");
        await CreateIndexIfMissingAsync(dbContext, "UserNameIndex", "AspNetUsers", "NormalizedUserName", unique: true);
        await CreateIndexIfMissingAsync(dbContext, "IX_StudentCourses_UserId", "StudentCourses", "UserId");
    }

    private static async Task AddColumnIfMissingAsync(
        AdvisorDbContext dbContext,
        string table,
        string column,
        string definition)
    {
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync($"""ALTER TABLE "{table}" ADD COLUMN "{column}" {definition};""");
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 1 && exception.Message.Contains("duplicate column name"))
        {
        }
    }

    private static async Task CreateIndexIfMissingAsync(
        AdvisorDbContext dbContext,
        string indexName,
        string table,
        string column,
        bool unique = false)
    {
        var uniqueSql = unique ? "UNIQUE " : string.Empty;
        await dbContext.Database.ExecuteSqlRawAsync($"""CREATE {uniqueSql}INDEX IF NOT EXISTS "{indexName}" ON "{table}" ("{column}");""");
    }
}
#pragma warning restore EF1002
