using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RegularPersonalDataTypesController : BaseOptionController<RegularPersonalDataType, ItSystem>
    {
        public RegularPersonalDataTypesController(IGenericRepository<RegularPersonalDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}