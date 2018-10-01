using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalSensistivePersonalDataTypesController : LocalOptionBaseController<LocalSensitivePersonalDataType, ItSystem, SensitivePersonalDataType>
    {
        public LocalSensistivePersonalDataTypesController(IGenericRepository<LocalSensitivePersonalDataType> repository, IAuthenticationService authService, IGenericRepository<SensitivePersonalDataType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}