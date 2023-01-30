using System;

namespace Core.DomainModel
{
    public interface IReadModel<TSourceEntity> : IHasId
    {
        int SourceEntityId { get; set; }
        Guid SourceEntityUuid { get; set; }
        TSourceEntity SourceEntity { get; set; }
    }
}
