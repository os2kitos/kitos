using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class KendoOrganizationalConfigurationMap : EntityTypeConfiguration<KendoOrganizationalConfiguration>
    {
        public KendoOrganizationalConfigurationMap()
        {
            
            Property(x => x.OverviewType)
                .IsRequired();

            Property(x => x.Configuration)
                .IsRequired();

            Property(x => x.OrganizationId)
                .IsRequired();
        }
    }
}
