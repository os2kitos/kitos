using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class PurchaseFormController : GenericOptionApiController<PurchaseFormType, ItContract, OptionDTO>
    {
        public PurchaseFormController(IGenericRepository<PurchaseFormType> repository)
            : base(repository)
        {
        }
    }
}
