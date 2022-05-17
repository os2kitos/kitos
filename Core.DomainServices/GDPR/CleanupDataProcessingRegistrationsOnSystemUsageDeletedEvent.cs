using System.Linq;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Repositories.GDPR;

namespace Core.DomainServices.GDPR
{
    public class CleanupDataProcessingRegistrationsOnSystemUsageDeletedEvent : IDomainEventHandler<EntityBeingDeletedEvent<ItSystemUsage>>
    {
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;
        private readonly IDataProcessingRegistrationSystemAssignmentService _assignmentService;

        public CleanupDataProcessingRegistrationsOnSystemUsageDeletedEvent(
            IDataProcessingRegistrationRepository dataProcessingRegistrationRepository, 
            IDataProcessingRegistrationSystemAssignmentService assignmentService)
        {
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
            _assignmentService = assignmentService;
        }

        public void Handle(EntityBeingDeletedEvent<ItSystemUsage> domainEvent)
        {
            var usage = domainEvent.Entity;
            foreach (var dataProcessingRegistration in usage.AssociatedDataProcessingRegistrations.ToList())
            {
                _assignmentService.RemoveSystem(dataProcessingRegistration, usage.ItSystemId);
                _dataProcessingRegistrationRepository.Update(dataProcessingRegistration);
            }
        }
    }
}
