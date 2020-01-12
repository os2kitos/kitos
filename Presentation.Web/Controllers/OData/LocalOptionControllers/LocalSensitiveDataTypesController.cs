using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalSensitiveDataTypesController : LocalOptionBaseController<LocalSensitiveDataType, ItSystemUsage, SensitiveDataType>
    {
        public LocalSensitiveDataTypesController(IGenericRepository<LocalSensitiveDataType> repository, IAuthenticationService authService, IGenericRepository<SensitiveDataType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
