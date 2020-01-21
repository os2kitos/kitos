using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalAgreementElementTypesController : LocalOptionBaseController<LocalAgreementElementType, ItContract, AgreementElementType>
    {
        public LocalAgreementElementTypesController(IGenericRepository<LocalAgreementElementType> repository, IGenericRepository<AgreementElementType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
