using Core.DomainModel.ItContract;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    public class ItContractRightController : GenericRightsController<ItContract, ItContractRight, ItContractRole>
    {
        public ItContractRightController(
            IGenericRepository<ItContractRight> rightRepository, 
            IGenericRepository<ItContract> objectRepository,
            IDomainEvents domainEvents) 
            : base(rightRepository, objectRepository, domainEvents)
        {
        }
    }
}
