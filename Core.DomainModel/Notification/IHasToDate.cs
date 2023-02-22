using System;

namespace Core.DomainModel.Notification
{
    public interface IHasToDate
    {
        DateTime? ToDate { get; set; }
    }
}
