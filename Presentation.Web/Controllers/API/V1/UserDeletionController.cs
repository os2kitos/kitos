using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/user/delete")]
    public class UserDeletionController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IEntityIdentityResolver _identityResolver;

        public UserDeletionController(IUserService userService, IEntityIdentityResolver identityResolver)
        {
            _userService = userService;
            _identityResolver = identityResolver;
        }

        /// <summary>
        /// Deletes user from the system
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public HttpResponseMessage Delete(int id)
        {
            var uuid = _identityResolver.ResolveUuid<User>(id);
            return uuid.IsNone
                ? NotFound()
                : _userService.DeleteUser(uuid.Value).Match(FromOperationError, Ok);
        }

        /// <summary>
        /// Deletes user from the organization or the system
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <param name="organizationId">The id of the current organization from which the user is to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}/{organizationId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public HttpResponseMessage Delete(int id, int organizationId)
        {
            var uuid = _identityResolver.ResolveUuid<User>(id);
            return uuid.IsNone
                ? NotFound()
                : _userService.DeleteUser(uuid.Value, organizationId).Match(FromOperationError, Ok);
        }
    }
}