using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.API
{
    public class ItContractRightsController : GenericRightsController<ItContract, ItContractRight, ItContractRole>
    {
        public ItContractRightsController(IGenericRepository<ItContractRight> rightRepository, IGenericRepository<ItContract> objectRepository) : base(rightRepository, objectRepository)
        {
        }
    }
}
