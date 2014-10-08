using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectRightsController : GenericRightsController<ItProject, ItProjectRight, ItProjectRole>
    {
        public ItProjectRightsController(IGenericRepository<ItProjectRight> rightRepository, IGenericRepository<ItProject> objectRepository) : base(rightRepository, objectRepository)
        {
        }
    }
}
