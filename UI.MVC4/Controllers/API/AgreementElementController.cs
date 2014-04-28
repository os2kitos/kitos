using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AgreementElementController : GenericOptionApiController<AgreementElement, Agreement, OptionDTO>
    {
        public AgreementElementController(IGenericRepository<AgreementElement> repository) 
            : base(repository)
        {
        }
    }
}
