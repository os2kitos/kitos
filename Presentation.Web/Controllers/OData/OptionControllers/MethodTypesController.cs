using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class MethodTypesController : BaseOptionController<MethodType, ItInterface>
    {
        public MethodTypesController(IGenericRepository<MethodType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}