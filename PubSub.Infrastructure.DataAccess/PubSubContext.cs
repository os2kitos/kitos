using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PubSub.Core.DomainModel.Subscriptions;

namespace PubSub.Infrastructure.DataAccess
{
    public class PubSubContext : DbContext
    {
        public PubSubContext(DbContextOptions<PubSubContext> options) : base(options)
        {
        }

        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new Mappings.SubscriptionMappingConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var ignorePendingModelChangesWarning =
                string.Equals(Environment.GetEnvironmentVariable("IgnorePendingModelChangesWarning"), "true", StringComparison.OrdinalIgnoreCase);

            if (ignorePendingModelChangesWarning)
            {
                optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
