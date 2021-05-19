using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystem;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class InterfaceDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItInterface>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;

        public InterfaceDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

        public void Handle(EntityDeletedEvent<ItInterface> domainEvent)
        {
            var interfaceDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == interfaceDeleted.Id && a.Type == ObjectType.itInterface).ToList();
            foreach (var advice in toBeDeleted)
            {
                _adviceRepository.DeleteByKeyWithReferencePreload(advice.Id);
            }
            _adviceRepository.Save();
        }
    }
}
