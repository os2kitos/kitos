using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItProjectStatusUpdatesController : BaseEntityController<ItProjectStatusUpdate>
    {
    public ItProjectStatusUpdatesController(IGenericRepository<ItProjectStatusUpdate>
        repository, IAuthenticationService authService)
        : base(repository, authService)
        {
        }
    }
}
