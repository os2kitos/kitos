using Infrastructure.Services.DomainEvents;
using Ninject;

namespace Infrastructure.Ninject.DomainServices
{
    /// <summary>
    /// DomainEvents
    /// <see cref="http://udidahan.com/2009/06/14/domain-events-salvation"/>
    /// <seealso cref="https://devblogs.microsoft.com/cesardelatorre/domain-events-vs-integration-events-in-domain-driven-design-and-microservices-architectures"/>
    /// <remarks>We have chosen to use a simpler in-memory version of the Domain Events implementation, since we do not have a service bus or u-service architecture</remarks>
    /// </summary>
    public class NinjectDomainEventsAdapter : IDomainEvents
    {
        private readonly IKernel _kernel;

        public NinjectDomainEventsAdapter(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// Raises the given domain event
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <param name="args"></param>
        public void Raise<TDomainEvent>(TDomainEvent args) where TDomainEvent : IDomainEvent
        {
            foreach (var handler in _kernel.GetAll<IDomainEventHandler<TDomainEvent>>())
            {
                handler.Handle(args);
            }
        }
    }
}
