using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalItContractRolesController : LocalOptionBaseController<LocalItContractRole, ItContractRight, ItContractRole>
    {
        public LocalItContractRolesController(IGenericRepository<LocalItContractRole> repository, IAuthenticationService authService, IGenericRepository<ItContractRole> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
