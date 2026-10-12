using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationRoleAssignmentReadModelMap : IEntityTypeConfiguration<DataProcessingRegistrationRoleAssignmentReadModel>
    {
        public void Configure(EntityTypeBuilder<DataProcessingRegistrationRoleAssignmentReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.RoleAssignments)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.UserFullName).IsRequired().HasMaxLength(UserConstraints.MaxNameLength);
            builder.HasIndex(x => x.UserFullName).HasDatabaseName("IX_UserFullName");

            builder.Property(x => x.UserId).IsRequired();
            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_UserId");

            builder.Property(x => x.RoleId).IsRequired();
            builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_RoleId");
        }
    }
}
