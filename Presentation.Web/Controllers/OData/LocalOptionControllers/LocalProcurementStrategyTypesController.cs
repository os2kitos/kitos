using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalProcurementStrategyTypesController : LocalOptionBaseController<LocalProcurementStrategyType, ItContract, ProcurementStrategyType>
    {
        public LocalProcurementStrategyTypesController(IGenericRepository<LocalProcurementStrategyType> repository, IAuthenticationService authService, IGenericRepository<ProcurementStrategyType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
