using System;
using System.Threading;
using Infrastructure.DataAccess;
using Npgsql;

namespace Tools.Test.Database.Model.Tasks
{
    public class DropDatabaseTask : DatabaseTask
    {
        private readonly string _connectionString;
        public DropDatabaseTask(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override bool Execute(KitosContext context)
        {
            return DropPostgreSqlDatabase();
        }

        private bool DropPostgreSqlDatabase()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
            var dbName = connectionStringBuilder.Database;
            if (string.IsNullOrWhiteSpace(dbName))
            {
                throw new InvalidOperationException("PostgreSQL connection string does not include a database name.");
            }

            // Check existence first: if the database is not there yet, nothing to drop.
            try
            {
                using var existenceCheck = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
                existenceCheck.Open();
            }
            catch (PostgresException ex) when (ex.SqlState == "3D000")
            {
                Console.WriteLine($"Database '{dbName}' does not exist, skipping drop.");
                return true;
            }

            // Connect to the postgres maintenance database to drop the target — same pattern
            // as the SQL Server version which connects to master.
            connectionStringBuilder.Database = "postgres";
            using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
            return DropPostgresDatabaseOnConnection(connection, dbName);
        }

        private static bool DropPostgresDatabaseOnConnection(NpgsqlConnection connection, string dbName)
        {
            const int maxAttempts = 15;
            const int delayMs = 5000;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var escapedDbName = dbName.Replace("\"", "\"\"");

                    using var terminateConnectionsCommand = connection.CreateCommand();
                    terminateConnectionsCommand.CommandTimeout = 60;
                    terminateConnectionsCommand.CommandText =
                        "SELECT pg_terminate_backend(pid) " +
                        "FROM pg_stat_activity " +
                        "WHERE datname = @dbName " +
                        "AND pid <> pg_backend_pid();";
                    terminateConnectionsCommand.Parameters.AddWithValue("dbName", dbName);
                    terminateConnectionsCommand.ExecuteNonQuery();

                    using var dropDatabaseCommand = connection.CreateCommand();
                    dropDatabaseCommand.CommandTimeout = 60;
                    dropDatabaseCommand.CommandText = $"DROP DATABASE IF EXISTS \"{escapedDbName}\";";
                    dropDatabaseCommand.ExecuteNonQuery();

                    Console.WriteLine($"Dropped database '{dbName}'");
                    return true;
                }
                catch (PostgresException postgresException)
                    when ((postgresException.SqlState == "55006" || postgresException.SqlState == "57P04") && attempt < maxAttempts)
                {
                    Console.WriteLine(
                        $"Attempt {attempt}/{maxAttempts} failed (PostgreSQL state {postgresException.SqlState}): {postgresException.MessageText}. Retrying in {delayMs}ms...");
                    Thread.Sleep(delayMs);
                }
            }

            throw new InvalidOperationException($"Failed to drop database '{dbName}' after {maxAttempts} attempts.");
        }

    }
}
