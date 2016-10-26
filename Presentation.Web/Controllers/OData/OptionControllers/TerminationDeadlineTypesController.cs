using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class TerminationDeadlineTypesController : BaseRoleController<TerminationDeadlineType, ItContract>
    {
        public TerminationDeadlineTypesController(IGenericRepository<TerminationDeadlineType> repository, IAuthenticationService authService)
            : base(repository, authService)
        { }
    }
}