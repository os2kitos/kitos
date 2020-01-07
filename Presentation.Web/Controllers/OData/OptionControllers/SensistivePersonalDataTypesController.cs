using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class SensistivePersonalDataTypesController : BaseOptionController<SensitivePersonalDataType, ItSystem>
    {
        public SensistivePersonalDataTypesController(IGenericRepository<SensitivePersonalDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}