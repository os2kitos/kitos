using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainServices.Repositories.Advice;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ContractDeletedAdvicesHandler : IDomainEventHandler<ContractDeleted>
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IAdviceService _adviceService;

        public ContractDeletedAdvicesHandler(IAdviceRepository adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(ContractDeleted domainEvent)
        {
            var contractDeleted = domainEvent.DeletedContract;
            var toBeDeleted = _adviceRepository.GetByRelationIdAndType(contractDeleted.Id, ObjectType.itContract).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
