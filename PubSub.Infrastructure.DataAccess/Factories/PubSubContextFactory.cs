using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using PubSub.Core.Abstractions.Helpers;

namespace PubSub.Infrastructure.DataAccess.Factories
{
    public class PubSubContextFactory : IDesignTimeDbContextFactory<PubSubContext>
    {
        public PubSubContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING");
            var optionsBuilder = new DbContextOptionsBuilder<PubSubContext>();
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.ConnectionStringBuilder.GssEncryptionMode = GssEncryptionMode.Disable;
            optionsBuilder.UseNpgsql(dataSourceBuilder.Build());

            return new PubSubContext(optionsBuilder.Options);
        }

    }
}
