using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AssignmentController : GenericApiController<Assignment, AssignmentDTO>
    {
        public AssignmentController(IGenericRepository<Assignment> repository) 
            : base(repository)
        {
        }
    }
}
