using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Users;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewRoleAssignmentReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewRoleAssignmentReadModel>
    {
        public ItSystemUsageOverviewRoleAssignmentReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.RoleAssignments)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);
            Property(x => x.UserFullName)
                .IsRequired()
                .HasMaxLength(UserConstraints.MaxNameLength)
                .HasIndexAnnotation("IX_UserFullName", 0);
            Property(x => x.UserId)
                .IsRequired()
                .HasIndexAnnotation("IX_UserId", 0);

            Property(x => x.RoleId)
                .IsRequired()
                .HasIndexAnnotation("IX_RoleId", 0);

            Property(x => x.Email)
                .HasMaxLength(UserConstraints.MaxEmailLength)
                .HasIndexAnnotation("IX_Email", 0);
        }
    }
}
