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
    public class ItProjectRightsController : GenericRightsController<ItProject, ItProjectRight, ItProjectRole>
    {
        public ItProjectRightsController(IGenericRepository<ItProjectRight> rightRepository, IGenericRepository<ItProject> objectRepository) : base(rightRepository, objectRepository)
        {
        }
    }
}
