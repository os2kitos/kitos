namespace Core.DomainModel
{
    public interface IReadModel<TSourceEntity>
    {
        public int Id { get; set; }
        public int SourceEntityId { get; set; }
        public TSourceEntity SourceEntity { get; set; }
    }
}
