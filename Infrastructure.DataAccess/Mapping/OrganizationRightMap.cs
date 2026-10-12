using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationRightMap : EntityMap<OrganizationRight>
    {
        public override void Configure(EntityTypeBuilder<OrganizationRight> builder)
        {
            base.Configure(builder);
            base.Configure(builder);

            builder.HasOne(right => right.Organization)
                .WithMany(obj => obj.Rights)
                .HasForeignKey(right => right.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.User)
                .WithMany(d => d.OrganizationRights)
                .HasForeignKey(t => t.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DefaultOrgUnit)
                .WithMany(d => d.DefaultUsers)
                .HasForeignKey(t => t.DefaultOrgUnitId);
        }
    }
}
