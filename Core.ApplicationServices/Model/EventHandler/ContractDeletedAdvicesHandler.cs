using System.Linq;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.Shared;
using Core.DomainServices.Repositories.Advice;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ContractDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItContract>>
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IAdviceService _adviceService;

        public ContractDeletedAdvicesHandler(IAdviceRepository adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<ItContract> domainEvent)
        {
            var contractDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.GetByRelationIdAndType(contractDeleted.Id, RelatedEntityType.itContract).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
