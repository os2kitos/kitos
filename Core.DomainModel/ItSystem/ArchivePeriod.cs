using System;
using Core.Abstractions.Types;

namespace Core.DomainModel.ItSystem
{
    public class ArchivePeriod : Entity, ISystemModule, IHasUuid
    {
        public ArchivePeriod()
        {
            Uuid = Guid.NewGuid();
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UniqueArchiveId { get; set; }
        public int ItSystemUsageId { get; set; }
        public ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }
        public bool Approved { get; set; }
        public Guid Uuid { get; set; }

        public Maybe<OperationError> UpdatePeriod(DateTime start, DateTime end)
        {
            if (start.Date > end.Date)
            {
                return new OperationError($"StartDate: {start.Date} cannot be before EndDate: {end.Date}", OperationFailure.BadInput);
            }
            StartDate = start;
            EndDate = end;
            return Maybe<OperationError>.None;
        }
    }
}
