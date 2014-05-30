using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public abstract class RightMap<TObject, TRight, TRole> : EntityMap<TRight>
        where TRight : Entity, IRight<TObject, TRight, TRole>
        where TObject : Entity, IHasRights<TRight>
        where TRole : Entity, IRoleEntity<TRight>
    {
        protected RightMap()
        {
            this.HasRequired(right => right.Object)
                .WithMany(obj => obj.Rights)
                .HasForeignKey(right => right.ObjectId);

            this.HasRequired(right => right.Role)
                .WithMany(role => role.References)
                .HasForeignKey(right => right.RoleId);

            this.HasRequired(right => right.User)
                .WithMany()
                .HasForeignKey(right => right.UserId);
        }
    }
}