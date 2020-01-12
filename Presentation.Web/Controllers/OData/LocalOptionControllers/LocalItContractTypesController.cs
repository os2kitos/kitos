using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalItContractTypesController : LocalOptionBaseController<LocalItContractType, ItContract, ItContractType>
    {
        public LocalItContractTypesController(IGenericRepository<LocalItContractType> repository, IAuthenticationService authService, IGenericRepository<ItContractType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
