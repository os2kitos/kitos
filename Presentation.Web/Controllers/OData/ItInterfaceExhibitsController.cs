using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItInterfaceExhibitsController : BaseEntityController<ItInterfaceExhibit>
    {
        public ItInterfaceExhibitsController(IGenericRepository<ItInterfaceExhibit> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
