using System;

namespace Core.DomainModel
{
    public interface IHasUuid
    {
        Guid Uuid { get; set; }
    }
}
