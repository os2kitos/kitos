using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Users;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Users;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/organizations/{organizationId}/administration/users/permissions")]
    public class UserAdministrationPermissionsController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserAdministrationPermissionsController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetPermissions(Guid organizationId)
        {
            return _userService
                .GetAdministrativePermissions(organizationId)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        private static UserAdministrationPermissionsDTO ToDTO(UserAdministrationPermissions permissions)
        {
            return new UserAdministrationPermissionsDTO { AllowDelete = permissions.AllowDelete };
        }
    }
}