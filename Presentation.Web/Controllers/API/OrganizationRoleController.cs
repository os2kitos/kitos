using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class OrganizationRoleController : GenericOptionApiController<OrganizationRole, OrganizationRight, RoleDTO>
    {
        private readonly IAdminService _adminService;

        public OrganizationRoleController(IGenericRepository<OrganizationRole> repository, IAdminService adminService) : base(repository)
        {
            _adminService = adminService;
        }

        public HttpResponseMessage GetLocalAdminRole(bool? getLocalAdminRole)
        {
            var dto = Map(_adminService.GetLocalAdminRole());

            return Ok(dto);
        }

        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Post(RoleDTO dto)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Put(int id, int organizationId, JObject jObject)
        {
            return NotAllowed();
        }
    }
}
