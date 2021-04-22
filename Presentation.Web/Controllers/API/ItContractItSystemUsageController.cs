using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
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

            item.ItSystemUsage.MainContract = item;
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

            // WARNING: force loading so setting it to null will be tracked
            var forceLoad = usage.MainContract;
            usage.MainContract = null;

            _domainEvent.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
            _usageRepository.Save();
            return Ok();
        }
    }
}
