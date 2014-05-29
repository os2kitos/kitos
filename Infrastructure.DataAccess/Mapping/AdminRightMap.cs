using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdminRightMap : EntityMap<AdminRight>
    {
        public AdminRightMap()
        {
            this.ToTable("AdminRight");

            this.HasRequired(right => right.Object)
                .WithMany(org => org.AdminRights)
                .HasForeignKey(right => right.ObjectId);

            this.HasRequired(right => right.Role)
                .WithMany(role => role.References)
                .HasForeignKey(right => right.RoleId);

            this.HasRequired(right => right.User)
                .WithMany(user => user.AdminRights)
                .HasForeignKey(right => right.UserId);
        }
    }
}