using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class CleanupDataProcessingRegistrationsOnSystemUsageDeletedEvent : IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>
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

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent)
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
