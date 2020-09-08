using Core.DomainModel.ItContract;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingAgreementRight : Entity, IRight<DataProcessingAgreement, DataProcessingAgreementRight, DataProcessingAgreementRole>, IDataProcessingModule
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual DataProcessingAgreementRole Role { get; set; }
        public virtual DataProcessingAgreement Object { get; set; }
    }
}