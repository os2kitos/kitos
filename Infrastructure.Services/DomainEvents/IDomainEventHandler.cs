namespace Infrastructure.Services.DomainEvents
{
    public interface IDomainEventHandler<in T> where T: IDomainEvent
    {
        void Handle(T domainEvent);
    }
}