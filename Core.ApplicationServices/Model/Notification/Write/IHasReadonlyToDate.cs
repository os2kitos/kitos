using System;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public interface IHasReadonlyToDate
    {
        DateTime? ToDate { get; }
    }
}
