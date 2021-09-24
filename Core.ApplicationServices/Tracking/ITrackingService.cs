using System;
using System.Linq;
using Core.DomainModel.Tracking;

namespace Core.ApplicationServices.Tracking
{
    public interface ITrackingService
    {
        IQueryable<LifeCycleTrackingEvent> QueryLifeCycleEvents(TrackedLifeCycleEventType? eventType = null, TrackedEntityType? trackedEntityType = null, DateTime? since = null);
    }
}
