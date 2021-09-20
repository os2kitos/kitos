namespace Core.DomainModel.Events
{
    public class NamedEntityChangedNameEvent<TEntity> : EntityUpdatedEvent<TEntity>
    {
        public NamedEntityChangedNameEvent(TEntity entity) 
            : base(entity)
        {

        }
    }
}
