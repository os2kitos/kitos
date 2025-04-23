using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PubSub.DataAccess.Factories
{
    public class PubSubContextFactory : IDesignTimeDbContextFactory<PubSubContext>
    {
        public PubSubContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING");

            var optionsBuilder = new DbContextOptionsBuilder<PubSubContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new PubSubContext(optionsBuilder.Options);
        }
    }
}
