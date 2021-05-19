using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class DataProcessingRegistrationDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<DataProcessingRegistration>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;

        public DataProcessingRegistrationDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

        public void Handle(EntityDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            var dataProcessingRegistration = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == dataProcessingRegistration.Id && a.Type == ObjectType.dataProcessingRegistration).ToList();
            foreach (var advice in toBeDeleted)
            {
                _adviceRepository.DeleteByKeyWithReferencePreload(advice.Id);
            }
            _adviceRepository.Save();
        }
    }
}
