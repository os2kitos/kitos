using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdminRightMap : EntityTypeConfiguration<AdminRight>
    {
        public AdminRightMap()
        {
            this.HasKey(right => new { right.ObjectId, right.RoleId, right.UserId });

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