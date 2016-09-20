using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractRightMap : RightMap<ItContract, ItContractRight, ItContractRole>
    {
        public ItContractRightMap()
        {
            this.HasRequired(x => x.User)
                .WithMany(x => x.ItContractRights)
                .HasForeignKey(x => x.UserId);
        }
    }
}
