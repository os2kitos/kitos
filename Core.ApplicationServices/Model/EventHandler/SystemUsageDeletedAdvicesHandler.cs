﻿using System.Linq;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices.Repositories.Advice;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class SystemUsageDeletedAdvicesHandler : IDomainEventHandler<EntityBeingDeletedEvent<ItSystemUsage>>
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IAdviceService _adviceService;

        public SystemUsageDeletedAdvicesHandler(IAdviceRepository adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityBeingDeletedEvent<ItSystemUsage> domainEvent)
        {
            var systemUsageDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.GetByRelationIdAndType(systemUsageDeleted.Id, RelatedEntityType.itSystemUsage).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
