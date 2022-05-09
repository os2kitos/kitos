﻿using System.Linq;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.Shared;
using Core.DomainServices.Repositories.Advice;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class DataProcessingRegistrationDeletedAdvicesHandler : IDomainEventHandler<EntityBeingDeletedEvent<DataProcessingRegistration>>
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IAdviceService _adviceService;

        public DataProcessingRegistrationDeletedAdvicesHandler(IAdviceRepository adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityBeingDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            var dataProcessingRegistration = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.GetByRelationIdAndType(dataProcessingRegistration.Id, RelatedEntityType.dataProcessingRegistration).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
