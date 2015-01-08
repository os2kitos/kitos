using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class TerminationDeadlineController : GenericOptionApiController<TerminationDeadline, ItContract, OptionDTO>
    {
        public TerminationDeadlineController(IGenericRepository<TerminationDeadline> repository) 
            : base(repository)
        {
        }
    }
}
