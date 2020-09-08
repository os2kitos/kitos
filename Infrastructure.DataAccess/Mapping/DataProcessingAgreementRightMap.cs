using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingAgreementRightMap : RightMap<DataProcessingAgreement, DataProcessingAgreementRight, DataProcessingAgreementRole>
    {
        public DataProcessingAgreementRightMap()
        {
            this.HasRequired(x => x.User)
                .WithMany(x => x.DataProcessingAgreementRights)
                .HasForeignKey(x => x.UserId);
        }
    }
}
