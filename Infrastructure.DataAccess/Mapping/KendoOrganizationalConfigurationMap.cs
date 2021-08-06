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

            Property(x => x.Version)
                .IsRequired();

            Property(x => x.OrganizationId)
                .IsRequired();

            //TODO: JMO- der mangler indexes på de properties du søger på - det er nok kun overviewtype du mangler index på men kig det lige efter
            HasMany(x => x.Columns)
                .WithRequired(x => x.KendoOrganizationalConfiguration)
                .HasForeignKey(x => x.KendoOrganizationalConfigurationId);
        }
    }
}
