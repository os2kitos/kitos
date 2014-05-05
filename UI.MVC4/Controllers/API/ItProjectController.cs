using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectController : GenericApiController<ItProject, int, ItProjectDTO>
    {
        public ItProjectController(IGenericRepository<ItProject> repository) 
            : base(repository)
        {
        }
    }
}