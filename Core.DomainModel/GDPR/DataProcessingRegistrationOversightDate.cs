using System;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistrationOversightDate : IHasUuid
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();

        public DateTime OversightDate { get; set; }
        public string OversightRemark { get; set; }

        public int ParentId { get; set; }
        public virtual DataProcessingRegistration Parent { get; set; }
    }
}
