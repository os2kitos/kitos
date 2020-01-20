using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItContractRightController : GenericRightsController<ItContract, ItContractRight, ItContractRole>
    {
        public ItContractRightController(
            IGenericRepository<ItContractRight> rightRepository, 
            IGenericRepository<ItContract> objectRepository) 
            : base(rightRepository, objectRepository)
        {
        }
    }
}
