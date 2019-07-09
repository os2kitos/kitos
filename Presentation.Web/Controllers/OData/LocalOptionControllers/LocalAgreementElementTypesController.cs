using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalAgreementElementTypesController : LocalOptionBaseController<LocalAgreementElementType, ItContract, AgreementElementType>
    {
        public LocalAgreementElementTypesController(IGenericRepository<LocalAgreementElementType> repository, IAuthenticationService authService, IGenericRepository<AgreementElementType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
