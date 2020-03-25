namespace Infrastructure.Services.DomainEvents
{
    public interface IDomainEventHandler<in T>
    {
        void Handle(T domainEvent);
    }
}