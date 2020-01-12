using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalSensistivePersonalDataTypesController : LocalOptionBaseController<LocalSensitivePersonalDataType, ItSystem, SensitivePersonalDataType>
    {
        public LocalSensistivePersonalDataTypesController(IGenericRepository<LocalSensitivePersonalDataType> repository, IAuthenticationService authService, IGenericRepository<SensitivePersonalDataType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}