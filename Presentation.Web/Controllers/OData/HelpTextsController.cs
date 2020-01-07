using Core.ApplicationServices;
using Core.DomainServices;
using Core.DomainModel;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    [ControllerEvaluationCompleted]
    public class HelpTextsController : BaseEntityController<HelpText>
    {
        public HelpTextsController(IGenericRepository<HelpText> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}