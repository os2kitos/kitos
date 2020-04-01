using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.SSO;

namespace Infrastructure.DataAccess.Mapping
{
    public class SsoOrganizationIdentityMap : EntityTypeConfiguration<SsoOrganizationIdentity>
    {
        public SsoOrganizationIdentityMap()
        {
            Property(x => x.ExternalUuid).HasUniqueIndexAnnotation("UX_" + nameof(SsoOrganizationIdentity.ExternalUuid), 0);
            HasRequired(x => x.Organization)
                .WithMany(x => x.SsoIdentities)
                .WillCascadeOnDelete(true);
        }
    }
}
