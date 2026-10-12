using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Presentation.Web.Hangfire;
using System;

namespace Presentation.Web.Infrastructure.Configuration
{
    public static class HangfireServiceCollectionExtensions
    {
        public static IServiceCollection AddKitosHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            var hangfireConnectionString = configuration.GetConnectionString("kitos_HangfireDB")
                ?? throw new InvalidOperationException("kitos_HangfireDB connection string is required");

            EnsureHangfireDatabaseCreated(hangfireConnectionString);

            services.AddHangfire(config =>
            {
                config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(hangfireConnectionString), new PostgreSqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true,
                        SchemaName = "hangfire"
                    });
            });

            services.AddSingleton<IBackgroundProcess>(provider => new KeepReadModelsInSyncProcess(provider));
            services.AddHangfireServer();

            return services;
        }

        private static void EnsureHangfireDatabaseCreated(string hangfireConnectionString)
        {
            var csb = new NpgsqlConnectionStringBuilder(hangfireConnectionString);
            var databaseName = csb.Database;
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException("Hangfire PostgreSQL connection string must include a database name.");

            csb.Database = "postgres";

            using var connection = new NpgsqlConnection(csb.ConnectionString);
            connection.Open();
            using var existsCmd = connection.CreateCommand();
            existsCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName";
            existsCmd.Parameters.AddWithValue("dbName", databaseName);

            var exists = existsCmd.ExecuteScalar() != null;
            if (!exists)
            {
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\"";
                createCmd.ExecuteNonQuery();
            }

            // Ensure schema exists; Hangfire.PostgreSql will create/migrate its own tables.
            csb.Database = databaseName;
            using var hangfireConnection = new NpgsqlConnection(csb.ConnectionString);
            hangfireConnection.Open();
            using var bootstrapCmd = hangfireConnection.CreateCommand();
            bootstrapCmd.CommandText = """
                CREATE SCHEMA IF NOT EXISTS hangfire;
                """;
            bootstrapCmd.ExecuteNonQuery();
        }

    }
}
