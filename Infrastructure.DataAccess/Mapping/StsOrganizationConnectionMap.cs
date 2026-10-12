using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationConnectionMap : EntityMap<StsOrganizationConnection>
    {
        public override void Configure(EntityTypeBuilder<StsOrganizationConnection> builder)
        {
            base.Configure(builder);
            base.Configure(builder);

            builder.HasOne(x => x.Organization)
                .WithOne(x => x.StsOrganizationConnection)
                .HasForeignKey<StsOrganizationConnection>(x => x.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Connected).IsRequired();
            builder.HasIndex(x => x.Connected).HasDatabaseName("IX_Connected");

            builder.Property(x => x.SubscribeToUpdates).IsRequired();
            builder.HasIndex(x => x.SubscribeToUpdates).HasDatabaseName("IX_Required");

            builder.HasIndex(x => x.DateOfLatestCheckBySubscription).HasDatabaseName("IX_DateOfLatestCheckBySubscription");
        }
    }
}
