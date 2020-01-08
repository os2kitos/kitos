using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    /// <summary>
    /// Gives access to relations between ItContract and ElementTypes
    /// Primarily used for reporting
    /// </summary>
    [PublicApi]
    public class ItContractAgreementElementTypesController : BaseController<ItContractAgreementElementTypes>
    {
        public ItContractAgreementElementTypesController(IGenericRepository<ItContractAgreementElementTypes> repository)
            : base(repository)
        {
        }
    }
}
