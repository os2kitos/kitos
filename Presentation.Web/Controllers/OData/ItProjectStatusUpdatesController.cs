using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.ApplicationServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItProjectStatusUpdatesController : BaseEntityController<ItProjectStatusUpdate>
    {
    public ItProjectStatusUpdatesController(IGenericRepository<ItProjectStatusUpdate>
        repository, IAuthenticationService authService)
        : base(repository)
        {
        }
    }
}
