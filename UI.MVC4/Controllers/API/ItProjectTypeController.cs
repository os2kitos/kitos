using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectTypeController : GenericOptionApiController<ItProjectType, ItProject, OptionDTO>
    {
        public ItProjectTypeController(IGenericRepository<ItProjectType> repository) 
            : base(repository)
        {
        }
    }
}