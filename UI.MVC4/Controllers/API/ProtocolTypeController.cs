using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class ProtocolTypeController : GenericApiController<ProjectType, int>
    {
        public ProtocolTypeController(IGenericRepository<ProjectType> repository) 
            : base(repository)
        {
        }
    }
}