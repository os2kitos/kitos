using Core.DomainModel.ItContract;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistrationRight : Entity, IRight<DataProcessingRegistration, DataProcessingRegistrationRight, DataProcessingRegistrationRole>, IDataProcessingModule
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual DataProcessingRegistrationRole Role { get; set; }
        public virtual DataProcessingRegistration Object { get; set; }
    }
}