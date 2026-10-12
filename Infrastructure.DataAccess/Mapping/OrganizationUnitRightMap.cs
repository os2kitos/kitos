using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationUnitRightMap : RightMap<OrganizationUnit, OrganizationUnitRight, OrganizationUnitRole>
    {
        public override void Configure(EntityTypeBuilder<OrganizationUnitRight> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.User)
                .WithMany(x => x.OrganizationUnitRights)
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
