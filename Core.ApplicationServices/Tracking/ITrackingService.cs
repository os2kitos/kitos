using System;
using System.Linq;
using Core.DomainModel.Tracking;

namespace Core.ApplicationServices.Tracking
{
    public interface ITrackingService
    {
        IQueryable<LifeCycleTrackingEvent> QueryLifeCycleEvents(DateTime? since = null);
    }
}
