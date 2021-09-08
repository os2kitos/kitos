namespace Core.DomainModel.Events
{
    public interface IDomainEventHandler<in T>
    {
        void Handle(T domainEvent);
    }
}