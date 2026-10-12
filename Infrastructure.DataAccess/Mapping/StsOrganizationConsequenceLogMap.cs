using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationConsequenceLogMap : EntityMap<StsOrganizationConsequenceLog>
    {
        public override void Configure(EntityTypeBuilder<StsOrganizationConsequenceLog> builder)
        {
            base.Configure(builder);
            base.Configure(builder);

            builder.HasOne(x => x.ChangeLog)
                .WithMany(x => x.Entries)
                .HasForeignKey(x => x.ChangeLogId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.ExternalUnitUuid).IsRequired();
            builder.HasIndex(x => x.ExternalUnitUuid).HasDatabaseName("IX_StsOrganizationConsequenceUuid");

            builder.Property(x => x.Type).IsRequired();
            builder.HasIndex(x => x.Type).HasDatabaseName("IX_StsOrganizationConsequenceType");

            builder.Property(x => x.Name).IsRequired();

            builder.Property(x => x.Description).IsRequired();
        }
    }
}
