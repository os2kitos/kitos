using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TerminationDeadlineController : GenericOptionApiController<TerminationDeadline, ItContract, OptionDTO>
    {
        public TerminationDeadlineController(IGenericRepository<TerminationDeadline> repository) 
            : base(repository)
        {
        }
    }
}