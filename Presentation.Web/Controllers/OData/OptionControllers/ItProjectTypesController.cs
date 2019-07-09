using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ItProjectTypesController : BaseOptionController<ItProjectType, ItProject>
    {
        public ItProjectTypesController(IGenericRepository<ItProjectType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}