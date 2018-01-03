using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class SensistivePersonalDataTypesController : BaseOptionController<SensitiveDataType, ItSystemUsage>
    {
        public SensistivePersonalDataTypesController(IGenericRepository<SensitiveDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}