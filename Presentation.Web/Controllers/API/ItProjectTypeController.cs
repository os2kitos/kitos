using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class ItProjectTypeController : GenericOptionApiController<ItProjectType, ItProject, OptionDTO>
    {
        public ItProjectTypeController(IGenericRepository<ItProjectType> repository) 
            : base(repository)
        {
        }
    }
}
