using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class AssignmentController : GenericContextAwareApiController<Assignment, AssignmentDTO>
    {
        public AssignmentController(IGenericRepository<Assignment> repository)
            : base(repository)
        {
        }
    }
}
