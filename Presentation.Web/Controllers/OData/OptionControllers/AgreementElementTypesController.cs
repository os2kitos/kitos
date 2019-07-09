using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AgreementElementTypesController: BaseOptionController<AgreementElementType, ItContract>
    {
        public AgreementElementTypesController(IGenericRepository<AgreementElementType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}