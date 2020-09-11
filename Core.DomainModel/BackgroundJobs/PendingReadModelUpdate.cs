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
