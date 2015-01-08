using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
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

        public override HttpResponseMessage Patch(int id, JObject obj)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Put(int id, JObject jObject)
        {
            return NotAllowed();
        }
    }
}
