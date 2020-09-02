namespace Core.DomainModel
{
    public interface IReadModel<TSourceEntity> : IHasId
    {
        int SourceEntityId { get; set; }
        TSourceEntity SourceEntity { get; set; }
    }
}
