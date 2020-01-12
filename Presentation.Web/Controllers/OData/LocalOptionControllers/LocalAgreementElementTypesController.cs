using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalAgreementElementTypesController : LocalOptionBaseController<LocalAgreementElementType, ItContract, AgreementElementType>
    {
        public LocalAgreementElementTypesController(IGenericRepository<LocalAgreementElementType> repository, IAuthenticationService authService, IGenericRepository<AgreementElementType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
