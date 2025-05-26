using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.KendoConfig;

namespace Infrastructure.DataAccess.Mapping
{
    public class KendoOrganizationalConfigurationMap : EntityTypeConfiguration<KendoOrganizationalConfiguration>
    {
        public KendoOrganizationalConfigurationMap()
        {
            
            Property(x => x.OverviewType)
                .IsRequired()
                .HasIndexAnnotation("KendoOrganizationalConfiguration_OverviewType", 0);

            Property(x => x.Version)
                .IsRequired();

            Property(x => x.OrganizationId)
                .IsRequired();

            HasMany(x => x.VisibleColumns)
                .WithRequired(x => x.KendoOrganizationalConfiguration)
                .HasForeignKey(x => x.KendoOrganizationalConfigurationId);
        }
    }
}
