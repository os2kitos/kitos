using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SensitiveDataTypesController : BaseOptionController<SensitiveDataType, ItSystemUsage>
    {
        public SensitiveDataTypesController(IGenericRepository<SensitiveDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}