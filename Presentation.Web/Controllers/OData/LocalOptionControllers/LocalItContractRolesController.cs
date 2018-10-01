using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalItContractRolesController : LocalOptionBaseController<LocalItContractRole, ItContractRight, ItContractRole>
    {
        public LocalItContractRolesController(IGenericRepository<LocalItContractRole> repository, IAuthenticationService authService, IGenericRepository<ItContractRole> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
