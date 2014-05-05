using System.Collections.Generic;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ProjectTypeController : GenericOptionApiController<ItProjectType, ItProject, OptionDTO>
    {
        public ProjectTypeController(IGenericRepository<ItProjectType> repository) 
            : base(repository)
        {
        }
    }
}