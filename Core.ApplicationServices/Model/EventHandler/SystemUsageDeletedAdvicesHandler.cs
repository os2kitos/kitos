using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Repositories.Advice;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class SystemUsageDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IAdviceService _adviceService;

        public SystemUsageDeletedAdvicesHandler(IAdviceRepository adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent)
        {
            var systemUsageDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.GetByRelationIdAndType(systemUsageDeleted.Id, ObjectType.itSystemUsage).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
