using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AdminRoleController : GenericOptionApiController<AdminRole, AdminRight, RoleDTO>
    {
        public AdminRoleController(IGenericRepository<AdminRole> repository) : base(repository)
        {
        }

        public override HttpResponseMessage Delete(int id)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Post(RoleDTO dto)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Patch(int id, Newtonsoft.Json.Linq.JObject obj)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Put(int id, RoleDTO dto)
        {
            return NotAllowed();
        }
    }
}
