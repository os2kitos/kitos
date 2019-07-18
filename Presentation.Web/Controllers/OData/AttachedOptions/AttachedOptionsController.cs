using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    public class AttachedOptionsController : BaseEntityController<AttachedOption>
    {
        public AttachedOptionsController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService)
               : base(repository, authService)
        {
        }
    }
}