using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalItContractRolesController : LocalOptionBaseController<LocalItContractRole, ItContractRight, ItContractRole>
    {
        public LocalItContractRolesController(IGenericRepository<LocalItContractRole> repository, IAuthenticationService authService, IGenericRepository<ItContractRole> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
