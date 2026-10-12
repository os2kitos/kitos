using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace Infrastructure.DataAccess
{
    /// <summary>
    /// Used by 'dotnet ef' tooling at design time (migrations, scaffolding).
    /// The connection string is read from the environment variable
    /// <c>ConnectionStrings__KitosContext</c> (standard .NET hierarchical config format).
    ///
    /// Set it before running any 'dotnet ef' command
    /// </summary>
    public class KitosContextDesignTimeFactory : IDesignTimeDbContextFactory<KitosContext>
    {
        private const string EnvVar = "ConnectionStrings__KitosContext";

        public KitosContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable(EnvVar);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    $"Design-time DB context requires the '{EnvVar}' environment variable to be set. " +
                    "Example: $env:ConnectionStrings__KitosContext = \"Host=localhost;Port=5432;Database=kitos;Username=postgres;Password=localNoSecret\"");

            var pgCsb = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = "dbo,public" };
            var optionsBuilder = new DbContextOptionsBuilder<KitosContext>();
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseNpgsql(pgCsb.ConnectionString,
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"))
                .ReplaceService<IMigrationsSqlGenerator, KitosNpgsqlMigrationsSqlGenerator>();

            return new KitosContext(optionsBuilder.Options);
        }
    }
}
