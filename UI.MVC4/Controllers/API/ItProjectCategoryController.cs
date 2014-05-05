using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectCategoryController : GenericOptionApiController<ItProjectCategory, ItProject, OptionDTO>
    {
        public ItProjectCategoryController(IGenericRepository<ItProjectCategory> repository) 
            : base(repository)
        {
        }
    }
}