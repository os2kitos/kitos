using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalItProjectTypesController : LocalOptionBaseController<LocalItProjectType, ItProject, ItProjectType>
    {
        public LocalItProjectTypesController(IGenericRepository<LocalItProjectType> repository, IAuthenticationService authService, IGenericRepository<ItProjectType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
