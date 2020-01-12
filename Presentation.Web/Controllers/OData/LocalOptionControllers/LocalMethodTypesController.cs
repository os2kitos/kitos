using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalMethodTypesController : LocalOptionBaseController<LocalMethodType, ItInterface, MethodType>
    {
        public LocalMethodTypesController(IGenericRepository<LocalMethodType> repository, IAuthenticationService authService, IGenericRepository<MethodType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
