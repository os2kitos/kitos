using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ProjectPhaseController : GenericOptionApiController<ProjectPhase, ProjectStatus>
    {
        public ProjectPhaseController(IGenericRepository<ProjectPhase> repository) : base(repository)
        {
        }
    }
}
