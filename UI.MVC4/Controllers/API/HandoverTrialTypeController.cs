using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class HandoverTrialTypeController : GenericOptionApiController<HandoverTrialType, HandoverTrial, OptionDTO>
    {
        public HandoverTrialTypeController(IGenericRepository<HandoverTrialType> repository) 
            : base(repository)
        {
        }
    }
}