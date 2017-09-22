using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalTerminationDeadlineTypesController : LocalOptionBaseController<LocalTerminationDeadlineType, ItContract, TerminationDeadlineType>
    {
        public LocalTerminationDeadlineTypesController(IGenericRepository<LocalTerminationDeadlineType> repository, IAuthenticationService authService, IGenericRepository<TerminationDeadlineType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
