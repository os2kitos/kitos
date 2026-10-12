using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationChangeLogMap : EntityMap<StsOrganizationChangeLog>
    {
        public override void Configure(EntityTypeBuilder<StsOrganizationChangeLog> builder)
        {
            base.Configure(builder);
            base.Configure(builder);

            builder.HasOne(x => x.StsOrganizationConnection)
                .WithMany(c => c.StsOrganizationChangeLogs)
                .HasForeignKey(x => x.StsOrganizationConnectionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.ResponsibleType).IsRequired();
            builder.HasIndex(x => x.ResponsibleType).HasDatabaseName("IX_ChangeLogResponsibleType");

            builder.Property(x => x.LogTime).IsRequired();
            builder.HasIndex(x => x.LogTime).HasDatabaseName("IX_LogTime");

            builder.HasOne(x => x.ResponsibleUser)
                .WithMany(x => x.StsOrganizationChangeLogs)
                .HasForeignKey(x => x.ResponsibleUserId);

            builder.HasIndex(x => x.ResponsibleUserId).HasDatabaseName("IX_ChangeLogName");
        }
    }
}
