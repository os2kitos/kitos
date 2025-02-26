using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Messages;
using Core.DomainModel.PublicMessage;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping;
using Presentation.Web.Infrastructure.Attributes;

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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<PublicMessageResponseDTO>))]
        public IHttpActionResult Get()
        {
            var publicMessages = _publicMessagesService.Read();
            var dtos = publicMessages.Select(ToDTO).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PublicMessageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult Post([FromBody] PublicMessageRequestDTO body)
        {
            if (body == null)
            {
                return BadRequest("Missing request body");
            }
            var parameters = _writeModelMapper.FromPOST(body);
            return _publicMessagesService.Create(parameters)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Update the public messages
        /// </summary>
        [HttpPatch]
        [Route("{messageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PublicMessageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult Patch([NonEmptyGuid] Guid messageUuid, [FromBody] PublicMessageRequestDTO body)
        {
            if (body == null)
            {
                return BadRequest("Missing request body");
            }

            var parameters = _writeModelMapper.FromPATCH(body);

            return _publicMessagesService.Patch(messageUuid, parameters)
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

        private static PublicMessageResponseDTO ToDTO(PublicMessage publicMessage)
        {
            return new PublicMessageResponseDTO(publicMessage);
        }
    }
}