using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class ItSystemGuidanceApiController : GenericApiController<ItSystemGuidanceApiController>
    {
        public ItSystemGuidanceApiController(IGenericRepository<ItSystemGuidanceApiController> repository) : base(repository)
        {
        }
    }
}
