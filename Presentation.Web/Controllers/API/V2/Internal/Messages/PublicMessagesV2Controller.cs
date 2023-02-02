using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Messages;
using Core.ApplicationServices.Model.Messages;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages
{
    [RoutePrefix("api/v2/internal/public-messages")]
    public class PublicMessagesV2Controller : InternalApiV2Controller
    {
        private readonly IPublicMessagesService _publicMessagesService;
        private readonly IResourcePermissionsResponseMapper _permissionsResponseMapper;
        private readonly IPublicMessagesWriteModelMapper _writeModelMapper;

        public PublicMessagesV2Controller(
            IPublicMessagesService publicMessagesService,
            IResourcePermissionsResponseMapper permissionsResponseMapper,
            IPublicMessagesWriteModelMapper writeModelMapper)
        {
            _publicMessagesService = publicMessagesService;
            _permissionsResponseMapper = permissionsResponseMapper;
            _writeModelMapper = writeModelMapper;
        }

        /// <summary>
        /// Returns public messages from KITOS
        /// </summary>
        [HttpGet]
        [Route]
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PublicMessagesResponseDTO))]
        public IHttpActionResult Get()
        {
            var publicMessages = _publicMessagesService.GetPublicMessages();
            var dto = ToDTO(publicMessages);
            return Ok(dto);
        }

        /// <summary>
        /// Update the public messages
        /// </summary>
        [HttpPatch]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PublicMessagesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult Patch([FromBody] PublicMessagesRequestDTO body)
        {
            if (body == null)
            {
                return BadRequest("Missing request body");
            }

            return _writeModelMapper
                .FromPATCH(body)
                .Transform(_publicMessagesService.UpdateMessages)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns permissions of the current api client in relation to the public texts resource
        /// </summary>
        [HttpGet]
        [Route("permissions")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourcePermissionsResponseDTO))]
        public IHttpActionResult GetPermissions()
        {
            return _publicMessagesService
                .GetPermissions()
                .Transform(_permissionsResponseMapper.Map)
                .Transform(Ok);
        }

        private static PublicMessagesResponseDTO ToDTO(PublicMessages publicMessages)
        {
            return new PublicMessagesResponseDTO
            {
                Guides = publicMessages.Guides,
                Misc = publicMessages.Misc,
                ContactInfo = publicMessages.ContactInfo,
                About = publicMessages.About,
                StatusMessages = publicMessages.StatusMessages
            };
        }
    }
}