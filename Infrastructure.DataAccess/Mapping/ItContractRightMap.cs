using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractRightMap : EntityTypeConfiguration<ItContractRight>
    {
        public ItContractRightMap()
        {
            this.HasKey(right => new { right.Object_Id, right.Role_Id, right.User_Id });

            this.ToTable("ItContractRight");

            this.HasRequired(right => right.Object)
                .WithMany(proj => proj.Rights)
                .HasForeignKey(right => right.Object_Id);

            this.HasRequired(right => right.Role)
                .WithMany(role => role.References)
                .HasForeignKey(right => right.Role_Id);

            this.HasRequired(right => right.User)
                .WithMany(user => user.ContractRights)
                .HasForeignKey(right => right.User_Id);
        }
    }
}