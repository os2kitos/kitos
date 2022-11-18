using System.Data.Entity.ModelConfiguration;
using System.Security.Cryptography.X509Certificates;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationChangeLogMap : EntityMap<StsOrganizationChangeLog>
    {
        public StsOrganizationChangeLogMap()
        {
            Property(x => x.Origin)
                .IsRequired()
                .HasIndexAnnotation("UX_ChangeLogOrigin");

            Property(x => x.Name)
                .IsOptional();
        }
    }
}
