using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItProjectTypeController : GenericOptionApiController<ItProjectType, ItProject, OptionDTO>
    {
        public ItProjectTypeController(IGenericRepository<ItProjectType> repository) 
            : base(repository)
        {
        }
    }
}
