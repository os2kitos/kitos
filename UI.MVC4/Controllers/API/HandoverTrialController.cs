using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class HandoverTrialController : GenericOptionApiController<HandoverTrial, ItContract, OptionDTO>
    {
        public HandoverTrialController(IGenericRepository<HandoverTrial> repository) 
            : base(repository)
        {
        }
    }
}
