using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationIdentityMap : EntityTypeConfiguration<StsOrganizationIdentity>
    {
        public StsOrganizationIdentityMap()
        {
            Property(x => x.ExternalUuid).HasUniqueIndexAnnotation("UX_" + nameof(StsOrganizationIdentity.ExternalUuid), 0);
            HasRequired(x => x.Organization)
                .WithMany(x => x.SsoIdentities)
                .WillCascadeOnDelete(true);
        }
    }
}
