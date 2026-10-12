using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractOverviewRoleAssignmentReadModelMap : IEntityTypeConfiguration<ItContractOverviewRoleAssignmentReadModel>
    {
        public void Configure(EntityTypeBuilder<ItContractOverviewRoleAssignmentReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_ItContract_Read_Role_Id");
            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_ItContract_Read_User_Id");

            builder.Property(x => x.UserFullName).HasMaxLength(UserConstraints.MaxNameLength);
            builder.HasIndex(x => x.UserFullName).HasDatabaseName("IX_ItContract_Read_User_Name");
        }
    }
}
