using Core.DomainModel.Shared;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewDataProcessingRegistrationReadModel
    {
        public int Id { get; set; }

        public int DataProcessingRegistrationId { get; set; }
        public string DataProcessingRegistrationName { get; set; }

        public YesNoIrrelevantOption? IsAgreementConcluded { get; set; }

        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
