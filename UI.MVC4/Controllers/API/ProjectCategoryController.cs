using System.Collections.Generic;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ProjectCategoryController : GenericOptionApiController<ItProjectCategory, ItProject, OptionDTO>
    {
        public ProjectCategoryController(IGenericRepository<ItProjectCategory> repository) 
            : base(repository)
        {
        }
    }
}