using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class SystemUsageDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceService _adviceService;

        public SystemUsageDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent)
        {
            var systemUsageDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == systemUsageDeleted.Id && a.Type == ObjectType.itSystemUsage).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
