using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class InterfaceDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItInterface>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceService _adviceService;

        public InterfaceDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<ItInterface> domainEvent)
        {
            var interfaceDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == interfaceDeleted.Id && a.Type == ObjectType.itInterface).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
