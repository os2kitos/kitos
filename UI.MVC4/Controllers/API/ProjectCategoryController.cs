using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ProjectCategoryController : GenericApiController<ProjectCategory, int, ProjectCategoryDTO>
    {
        public ProjectCategoryController(IGenericRepository<ProjectCategory> repository) 
            : base(repository)
        {
        }
    }
}