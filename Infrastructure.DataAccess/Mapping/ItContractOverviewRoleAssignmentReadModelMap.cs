using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Users;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractOverviewRoleAssignmentReadModelMap : EntityTypeConfiguration<ItContractOverviewRoleAssignmentReadModel>
    {
    public ItContractOverviewRoleAssignmentReadModelMap()
    {
        HasKey(x => x.Id);
        Property(x => x.RoleId)
            .HasIndexAnnotation("IX_ItContract_Read_Role_Id");

        Property(x => x.UserId)
            .HasIndexAnnotation("IX_ItContract_Read_User_Id");

        Property(x => x.UserFullName)
            .HasMaxLength(UserConstraints.MaxNameLength)
            .HasIndexAnnotation("IX_ItContract_Read_User_Name");
    }
}
}
