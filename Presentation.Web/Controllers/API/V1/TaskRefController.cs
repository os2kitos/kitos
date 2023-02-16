using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class TaskRefController : GenericHierarchyApiController<TaskRef, TaskRefDTO>
    {
        public TaskRefController(IGenericRepository<TaskRef> repository)
            : base(repository)
        {
        }
    }
}
