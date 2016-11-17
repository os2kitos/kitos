using Core.ApplicationServices;
using Core.DomainServices;
using Core.DomainModel;

namespace Presentation.Web.Controllers.OData
{
    public class HelpTextsController : BaseEntityController<HelpText>
    {
        public HelpTextsController(IGenericRepository<HelpText> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}