namespace Infrastructure.DataAccess.Mapping
{
    using Core.DomainModel.ItContract;

    public class ItContractRemarkMap : EntityMap<ItContractRemark>
    {
        public ItContractRemarkMap()
        {
            ToTable("ItContractRemarks");

            HasRequired(c => c.ItContract).WithRequiredDependent(r => r.Remark);
        }
    }
}
