using System;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistrationOversightDate : IHasUuid
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();

        public DateTime OversightDate { get; set; }
        public string OversightRemark { get; set; }

        public string OversightReportLink { get; set; }
        public string OversightReportLinkName { get; set; }

        public int? OversightOptionId { get; set; }
        public virtual DataProcessingOversightOption OversightOption { get; set; }

        public int ParentId { get; set; }
        public virtual DataProcessingRegistration Parent { get; set; }
    }
}
