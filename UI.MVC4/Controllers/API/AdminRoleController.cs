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
        private readonly IAdminService _adminService;

        public AdminRoleController(IGenericRepository<AdminRole> repository, IAdminService adminService) : base(repository)
        {
            _adminService = adminService;
        }

        public HttpResponseMessage GetLocalAdminRole(bool? getLocalAdminRole)
        {
            var dto = Map(_adminService.GetLocalAdminRole());

            return Ok(dto);
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
