namespace Infrastructure.Services.DomainEvents
{
    public class NamedEntityChangedNameEvent<TEntity> : EntityUpdatedEvent<TEntity>
    {
        public NamedEntityChangedNameEvent(TEntity entity) 
            : base(entity)
        {

        }
    }
}
