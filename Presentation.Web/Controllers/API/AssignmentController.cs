using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class AssignmentController : GenericApiController<Assignment, AssignmentDTO>
    {
        public AssignmentController(IGenericRepository<Assignment> repository) 
            : base(repository)
        {
        }
    }
}
