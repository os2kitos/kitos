using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalHandoverTrialTypesController : LocalOptionBaseController<LocalHandoverTrialType, HandoverTrial, HandoverTrialType>
    {
        public LocalHandoverTrialTypesController(IGenericRepository<LocalHandoverTrialType> repository, IAuthenticationService authService, IGenericRepository<HandoverTrialType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
