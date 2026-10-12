using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public abstract class RightMap<TObject, TRight, TRole> : EntityMap<TRight>
        where TRight : Entity, IRight<TObject, TRight, TRole>
        where TObject : HasRightsEntity<TObject, TRight, TRole>
        where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
    {
        public override void Configure(EntityTypeBuilder<TRight> builder)
        {
            base.Configure(builder);

            builder.HasOne(right => right.Object)
                .WithMany(obj => obj.Rights)
                .HasForeignKey(right => right.ObjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(right => right.Role)
                .WithMany(role => role.References)
                .HasForeignKey(right => right.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(right => right.User)
                .WithMany()
                .HasForeignKey(right => right.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
