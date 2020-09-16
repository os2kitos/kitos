using System;

namespace Core.DomainModel.BackgroundJobs
{
    public enum PendingReadModelUpdateSourceCategory
    {
        DataProcessingAgreement = 0,
        DataProcessingAgreement_User = 1
    }

    public class PendingReadModelUpdate
    {
        protected PendingReadModelUpdate()
        {

        }

        public static PendingReadModelUpdate Create(IHasId source, PendingReadModelUpdateSourceCategory category)
        {
            return new PendingReadModelUpdate()
            {
                CreatedAt = DateTime.UtcNow,
                SourceId = source.Id,
                Category = category
            };
        }

        public int Id { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PendingReadModelUpdateSourceCategory Category { get; set; }
    }
}
