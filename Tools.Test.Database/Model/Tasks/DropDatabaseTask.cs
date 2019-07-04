using System;
using System.Data.SqlClient;

namespace Tools.Test.Database.Model.Tasks
{
    public class DropDatabaseTask : DatabaseTask
    {
        private readonly string _connectionString;

        public DropDatabaseTask(string connectionString)
        : base(connectionString)
        {
            _connectionString = connectionString;
        }

        public override bool Execute()
        {
            //Create a new connection string without initial catalog so that db can be dropped
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            var dbName = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.Remove("Initial Catalog");

            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (var sqlCommand = connection.CreateCommand())
                    {
                        var sqlToDropDb =
                            $"alter database [{dbName}] set single_user with rollback immediate; " +
                            $"drop database [{dbName}]; ";

                        sqlCommand.CommandText = sqlToDropDb;
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }

            return true;
        }
    }
}
