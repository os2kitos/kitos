using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class AgreementElementController : GenericOptionApiController<AgreementElementType, ItContract, OptionDTO>
    {
        public AgreementElementController(IGenericRepository<AgreementElementType> repository)
            : base(repository)
        {
        }
    }
}
