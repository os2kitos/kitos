using Core.DomainModel.SSO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class SsoUserIdentityMap : IEntityTypeConfiguration<SsoUserIdentity>
    {
        public void Configure(EntityTypeBuilder<SsoUserIdentity> builder)
        {
            builder.HasIndex(x => x.ExternalUuid).IsUnique().HasDatabaseName("UX_" + nameof(SsoUserIdentity.ExternalUuid));

            builder.HasOne(x => x.User)
                .WithMany(x => x.SsoIdentities)
                .HasForeignKey("User_Id")
                .IsRequired()
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
