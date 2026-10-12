using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemRightMap : RightMap<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public override void Configure(EntityTypeBuilder<ItSystemRight> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.User)
                .WithMany(x => x.ItSystemRights)
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ObjectId)
                .HasDatabaseName("IX_ItSystemRights_ObjectId");
        }
    }
}
