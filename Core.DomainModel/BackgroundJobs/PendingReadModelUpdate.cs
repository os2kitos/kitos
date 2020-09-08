using System;
using Core.DomainModel.GDPR;

namespace Core.DomainModel.BackgroundJobs
{
    public enum PendingReadModelUpdateSourceCategory
    {
        DataProcessingAgreement = 0
    }
    public class PendingReadModelUpdate
    {
        protected PendingReadModelUpdate()
        {

        }

        public static PendingReadModelUpdate From(DataProcessingAgreement source)
        {
            return new PendingReadModelUpdate
            {
                CreatedAt = DateTime.UtcNow,
                SourceId = source.Id,
                Category = PendingReadModelUpdateSourceCategory.DataProcessingAgreement
            };
        }

        public int Id { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PendingReadModelUpdateSourceCategory Category { get; set; }
    }
}
