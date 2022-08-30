using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractOverviewRoleAssignmentReadModelMap : EntityTypeConfiguration<ItContractOverviewRoleAssignmentReadModel>
    {
    public ItContractOverviewRoleAssignmentReadModelMap()
    {
        HasKey(x => x.Id);
        Property(x => x.Id)
            .HasIndexAnnotation("IX_ItContract_Read_Role_Id");

        Property(x => x.UserId)
            .HasIndexAnnotation("IX_ItContract_Read_User_Id");
    }
}
}
