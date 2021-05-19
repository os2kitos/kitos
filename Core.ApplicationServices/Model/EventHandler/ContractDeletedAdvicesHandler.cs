using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ContractDeletedAdvicesHandler : IDomainEventHandler<ContractDeleted>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceService _adviceService;

        public ContractDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(ContractDeleted domainEvent)
        {
            var contractDeleted = domainEvent.DeletedContract;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == contractDeleted.Id && a.Type == ObjectType.itContract).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
