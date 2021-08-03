using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class TaskRefController : GenericHierarchyApiController<TaskRef, TaskRefDTO>
    {
        public TaskRefController(IGenericRepository<TaskRef> repository)
            : base(repository)
        {
        }
    }
}
