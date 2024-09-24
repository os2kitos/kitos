using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System;
using System.Web.Http;
using Core.ApplicationServices.Users.Write;
using Presentation.Web.Controllers.API.V2.Internal.Users.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using Presentation.Web.Models.API.V2.Request.User;
using System.Web.Http.Results;
using Core.ApplicationServices.Model.Users;

namespace Presentation.Web.Controllers.API.V2.Internal.Users
{
    /// <summary>
    /// Internal API for the users in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organization/{organizationUuid}/users")]
    public class UsersInternalV2Controller : InternalApiV2Controller
    {
        private readonly IUserWriteModelMapper _writeModelMapper;
        private readonly IUserWriteService _userWriteService;
        private readonly IUserResponseModelMapper _userResponseModelMapper;

        public UsersInternalV2Controller(IUserWriteModelMapper writeModelMapper, 
            IUserWriteService userWriteService, 
            IUserResponseModelMapper userResponseModelMapper)
        {
            _writeModelMapper = writeModelMapper;
            _userWriteService = userWriteService;
            _userResponseModelMapper = userResponseModelMapper;
        }

        [Route("create")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(UserResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult CreateUnit([NonEmptyGuid] Guid organizationUuid, [FromBody] CreateUserRequestDTO parameters)
        {
            return _userWriteService.Create(organizationUuid, _writeModelMapper.FromPOST(parameters))
                .Select(_userResponseModelMapper.ToUserResponseDTO)
                .Match(MapUserCreatedResponse, FromOperationError);
        }

        [Route("permissions")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserCollectionPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetCollectionPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            return _userWriteService.GetCollectionPermissions(organizationUuid)
                .Select(MapUserCollectionPermissionsResponseDto)
                .Match(Ok, FromOperationError);
        }

        private CreatedNegotiatedContentResult<UserResponseDTO> MapUserCreatedResponse(UserResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }

        private UserCollectionPermissionsResponseDTO MapUserCollectionPermissionsResponseDto(
            UserCollectionPermissionsResult permissions)
        {
            return new UserCollectionPermissionsResponseDTO(permissions.Create, permissions.Edit, permissions.Delete);

        }
    }
}