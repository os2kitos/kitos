using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItContract.DomainEvents;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class ContractDeletedAdvicesHandler : IDomainEventHandler<ContractDeleted>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;

        public ContractDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

        public void Handle(ContractDeleted domainEvent)
        {
            var contractDeleted = domainEvent.DeletedContract;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == contractDeleted.Id && a.Type == ObjectType.itContract).ToList();
            foreach (var advice in toBeDeleted)
            {
                _adviceRepository.DeleteByKeyWithReferencePreload(advice.Id);
            }
        }
    }
}
