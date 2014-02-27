using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class ProjectTypeController : GenericApiController<ProjectType, int>
    {
        public ProjectTypeController(IGenericRepository<ProjectType> repository) 
            : base(repository)
        {
        }
    }
}