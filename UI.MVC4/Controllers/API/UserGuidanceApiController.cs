using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Text;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class UserGuidanceApiController : GenericApiController<UserGuidance>
    {
        public UserGuidanceApiController(IGenericRepository<UserGuidance> repository) : base(repository)
        {
        }
    }
}
