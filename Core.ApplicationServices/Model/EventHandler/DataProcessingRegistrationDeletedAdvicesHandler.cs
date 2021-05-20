using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainServices.Repositories.Advice;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class DataProcessingRegistrationDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<DataProcessingRegistration>>
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IAdviceService _adviceService;

        public DataProcessingRegistrationDeletedAdvicesHandler(IAdviceRepository adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            var dataProcessingRegistration = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.GetByRelationIdAndType(dataProcessingRegistration.Id, ObjectType.dataProcessingRegistration).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
