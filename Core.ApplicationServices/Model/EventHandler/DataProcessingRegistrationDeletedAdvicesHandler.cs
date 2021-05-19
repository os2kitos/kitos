using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class DataProcessingRegistrationDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<DataProcessingRegistration>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceService _adviceService;

        public DataProcessingRegistrationDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            var dataProcessingRegistration = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == dataProcessingRegistration.Id && a.Type == ObjectType.dataProcessingRegistration).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
