using System;

namespace Core.DomainModel.BackgroundJobs
{
    public enum PendingReadModelUpdateSourceCategory
    {
        DataProcessingRegistration = 0,
        DataProcessingRegistration_User = 1,
        DataProcessingRegistration_ItSystem = 2,
        DataProcessingRegistration_Organization = 3,
        DataProcessingRegistration_BasisForTransfer = 4,
        DataProcessingRegistration_DataResponsible = 5,
        DataProcessingRegistration_OversightOption = 6,
        DataProcessingRegistration_ItContract = 7,
        ItSystemUsage = 8,
        ItSystemUsage_ItSystem = 9,
        ItSystemUsage_User = 10,
        ItSystemUsage_OrganizationUnit = 11,
        ItSystemUsage_Organization = 12,
        ItSystemUsage_BusinessType = 13,
        ItSystemUsage_TaskRef = 14,
        ItSystemUsage_Contract = 15,
        ItSystemUsage_Project = 16,
        ItSystemUsage_DataProcessingRegistration = 17
    }

    public class PendingReadModelUpdate : IHasId
    {
        protected PendingReadModelUpdate()
        {

        }

        public static PendingReadModelUpdate Create(IHasId source, PendingReadModelUpdateSourceCategory category)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return Create(source.Id, category);
        }

        public static PendingReadModelUpdate Create(int sourceId, PendingReadModelUpdateSourceCategory category)
        {
            return new PendingReadModelUpdate
            {
                CreatedAt = DateTime.UtcNow,
                SourceId = sourceId,
                Category = category
            };
        }

        public int Id { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PendingReadModelUpdateSourceCategory Category { get; set; }
    }
}
