using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItContractRightController : GenericRightsController<ItContract, ItContractRight, ItContractRole>
    {
        public ItContractRightController(
            IGenericRepository<ItContractRight> rightRepository, 
            IGenericRepository<ItContract> objectRepository,
            IAuthorizationContext authorizationContext) 
            : base(rightRepository, objectRepository, authorizationContext)
        {
        }
    }
}
