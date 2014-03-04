using System.Collections.Generic;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class ProjectTypeController : GenericApiController<ProjectType, int, ProjectTypeDTO>
    {
        public ProjectTypeController(IGenericRepository<ProjectType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<ProjectType> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}