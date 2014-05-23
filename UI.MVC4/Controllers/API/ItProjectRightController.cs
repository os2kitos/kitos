using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectRightController : GenericRightController<ItProjectRight, ItProject, ItProjectRole>
    {
        public ItProjectRightController(IGenericRepository<ItProjectRight> repository, 
            IGenericRepository<ItProject> projectRepository) : base(repository, projectRepository)
        {
        }
    }
}
