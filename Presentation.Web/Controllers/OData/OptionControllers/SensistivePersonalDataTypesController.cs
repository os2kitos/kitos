using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class SensistivePersonalDataTypesController : BaseOptionController<SensitivePersonalDataType, ItSystem>
    {
        public SensistivePersonalDataTypesController(IGenericRepository<SensitivePersonalDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}