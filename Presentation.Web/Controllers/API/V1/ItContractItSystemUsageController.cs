using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    public class ItContractItSystemUsageController : BaseApiController
    {
        private readonly IGenericRepository<ItContractItSystemUsage> _repository;
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IDomainEvents _domainEvent;

        public ItContractItSystemUsageController(
            IGenericRepository<ItContractItSystemUsage> repository,
            IGenericRepository<ItSystemUsage> usageRepository,
            IDomainEvents domainEvent)
        {
            _repository = repository;
            _usageRepository = usageRepository;
            _domainEvent = domainEvent;
        }

        public HttpResponseMessage PostMainContract(int contractId, int usageId)
        {
            var item = _repository.GetByKey(new object[] { contractId, usageId });

            if (item == null)
                return NotFound();

            if (!AllowModify(item.ItSystemUsage))
            {
                return Forbidden();
            }

            var error = item.ItSystemUsage.SetMainContract(item.ItContract);
            
            if (error.HasValue)
                return FromOperationError(error.Value);

            _domainEvent.Raise(new EntityUpdatedEvent<ItSystemUsage>(item.ItSystemUsage));
            _repository.Save();
            return Ok();
        }

        public HttpResponseMessage DeleteMainContract(int usageId)
        {
            var usage = _usageRepository.GetByKey(usageId);

            if (usage == null)
                return NotFound();

            if (!AllowModify(usage))
            {
                return Forbidden();
            }

            usage.ResetMainContract();

            _domainEvent.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
            _usageRepository.Save();
            return Ok();
        }
    }
}
