using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Users;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationRoleAssignmentReadModelMap : EntityTypeConfiguration<DataProcessingRegistrationRoleAssignmentReadModel>
    {
        public DataProcessingRegistrationRoleAssignmentReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.RoleAssignments)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(false);
            Property(x=>x.UserFullName)
                .IsRequired()
                .HasMaxLength(UserConstraints.MaxNameLength)
                .HasIndexAnnotation("IX_UserFullName", 0);
            Property(x => x.UserId)
                .IsRequired()
                .HasIndexAnnotation("IX_UserId", 0);

            Property(x => x.RoleId)
                .IsRequired()
                .HasIndexAnnotation("IX_RoleId", 0);
        }
    }
}
