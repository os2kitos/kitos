using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RegisterTypesController : BaseOptionController<RegisterType, ItSystemUsage>
    {
        public RegisterTypesController(IGenericRepository<RegisterType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}