using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class RegularPersonalDataTypesController : BaseOptionController<RegularPersonalDataType, ItSystem>
    {
        public RegularPersonalDataTypesController(IGenericRepository<RegularPersonalDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}