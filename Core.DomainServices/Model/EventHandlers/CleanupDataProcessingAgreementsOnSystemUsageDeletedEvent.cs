using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class CleanupDataProcessingAgreementsOnSystemUsageDeletedEvent : IDomainEventHandler<EntityDeletedEvent<ItSystemUsage>>
    {
        private readonly IDataProcessingAgreementRepository _dataProcessingAgreementRepository;
        private readonly IDataProcessingAgreementSystemAssignmentService _assignmentService;

        public CleanupDataProcessingAgreementsOnSystemUsageDeletedEvent(
            IDataProcessingAgreementRepository dataProcessingAgreementRepository, 
            IDataProcessingAgreementSystemAssignmentService assignmentService)
        {
            _dataProcessingAgreementRepository = dataProcessingAgreementRepository;
            _assignmentService = assignmentService;
        }

        public void Handle(EntityDeletedEvent<ItSystemUsage> domainEvent)
        {
            var usage = domainEvent.Entity;
            foreach (var dataProcessingAgreement in usage.AssociatedDataProcessingAgreements.ToList())
            {
                _assignmentService.RemoveSystem(dataProcessingAgreement, usage.ItSystemId);
                _dataProcessingAgreementRepository.Update(dataProcessingAgreement);
            }
        }
    }
}
