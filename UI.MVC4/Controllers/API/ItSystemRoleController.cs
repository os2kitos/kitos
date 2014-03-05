using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemRoleController : GenericOptionApiController<ItSystemRole, ItSystemRight>
    {
        public ItSystemRoleController(IGenericRepository<ItSystemRole> repository) : base(repository)
        {
        }
    }
}
