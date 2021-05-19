using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class SystemUsageDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;

        public SystemUsageDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent)
        {
            var systemUsageDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == systemUsageDeleted.Id && a.Type == ObjectType.itSystemUsage).ToList();
            foreach (var advice in toBeDeleted)
            {
                _adviceRepository.DeleteByKeyWithReferencePreload(advice.Id);
            }
        }
    }
}
