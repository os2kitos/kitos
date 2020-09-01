namespace Core.DomainModel
{
    public interface IReadModel<TSourceEntity> : IHasId
    {
        public int SourceEntityId { get; set; }
        public TSourceEntity SourceEntity { get; set; }
    }
}
