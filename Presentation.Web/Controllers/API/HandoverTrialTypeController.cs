using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class HandoverTrialTypeController : GenericOptionApiController<HandoverTrialType, HandoverTrial, OptionDTO>
    {
        public HandoverTrialTypeController(IGenericRepository<HandoverTrialType> repository) 
            : base(repository)
        {
        }
    }
}
                                        