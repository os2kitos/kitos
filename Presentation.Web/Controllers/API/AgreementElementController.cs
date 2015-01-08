using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class AgreementElementController : GenericOptionApiController<AgreementElement, ItContract, OptionDTO>
    {
        public AgreementElementController(IGenericRepository<AgreementElement> repository) 
            : base(repository)
        {
        }
    }
}
