using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ItContractRolesController : BaseOptionController<ItContractRole, ItContractRight>
    {
        public ItContractRolesController(IGenericRepository<ItContractRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
