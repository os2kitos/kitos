using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class ProtocolTypeController : GenericApiController<ProjectType, int, ProjectTypeDTO>
    {
        public ProtocolTypeController(IGenericRepository<ProjectType> repository) 
            : base(repository)
        {
        }
    }
}