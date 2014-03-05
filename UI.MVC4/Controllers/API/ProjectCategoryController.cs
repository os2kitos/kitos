using System.Collections.Generic;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ProjectCategoryController : GenericOptionApiController<ProjectCategory, ItProject>
    {
        public ProjectCategoryController(IGenericRepository<ProjectCategory> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<ProjectCategory> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}