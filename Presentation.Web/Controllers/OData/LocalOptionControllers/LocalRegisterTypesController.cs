using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalRegisterTypesController : LocalOptionBaseController<LocalRegisterType, ItSystemUsage, RegisterType>
    {
        public LocalRegisterTypesController(IGenericRepository<LocalRegisterType> repository, IAuthenticationService authService, IGenericRepository<RegisterType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}