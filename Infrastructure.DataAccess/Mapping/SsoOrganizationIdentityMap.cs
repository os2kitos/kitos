using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.SSO;

namespace Infrastructure.DataAccess.Mapping
{
    public class SsoUserIdentityMap : EntityTypeConfiguration<SsoUserIdentity>
    {
        public SsoUserIdentityMap()
        {
            Property(x => x.ExternalUuid).HasUniqueIndexAnnotation("UX_" + nameof(SsoUserIdentity.ExternalUuid), 0);
            HasRequired(x => x.User)
                .WithMany(x => x.SsoIdentities)
                .WillCascadeOnDelete(true);
        }
    }
}
