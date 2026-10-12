using Core.DomainModel.KendoConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class KendoOrganizationalConfigurationMap : IEntityTypeConfiguration<KendoOrganizationalConfiguration>
    {
        public void Configure(EntityTypeBuilder<KendoOrganizationalConfiguration> builder)
        {
            builder.Property(x => x.OverviewType).IsRequired();
            builder.HasIndex(x => x.OverviewType).HasDatabaseName("KendoOrganizationalConfiguration_OverviewType");

            builder.Property(x => x.Version).IsRequired();

            builder.Property(x => x.OrganizationId).IsRequired();

            builder.HasMany(x => x.VisibleColumns)
                .WithOne(x => x.KendoOrganizationalConfiguration)
                .HasForeignKey(x => x.KendoOrganizationalConfigurationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
