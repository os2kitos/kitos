using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Interface.Write;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Response.Interface;
using Presentation.Web.Models.API.V2.SharedProperties;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces
{
    [RoutePrefix("api/v2")]
    public class ItInterfaceV2Controller : ExternalBaseController
    {
        private readonly IItInterfaceRightsHolderService _rightsHolderService;
        private readonly IItInterfaceService _itInterfaceService;
        private readonly IItInterfaceWriteModelMapper _writeModelMapper;
        private readonly IItInterfaceWriteService _writeService;
        private readonly IItInterfaceResponseMapper _responseMapper;

        public ItInterfaceV2Controller(
            IItInterfaceRightsHolderService rightsHolderService, 
            IItInterfaceService itInterfaceService, 
            IItInterfaceWriteModelMapper writeModelMapper,
            IItInterfaceWriteService writeService,
            IItInterfaceResponseMapper responseMapper)
        {
            _rightsHolderService = rightsHolderService;
            _itInterfaceService = itInterfaceService;
            _writeModelMapper = writeModelMapper;
            _writeService = writeService;
            _responseMapper = responseMapper;
        }

        /// <summary>
        /// Creates a new IT-Interface based on given input values
        /// </summary>
        /// <param name="request">A collection of specific IT-Interface values</param>
        /// <returns>Location header is set to uri for newly created IT-Interface</returns>
        [HttpPost]
        [Route("it-interfaces")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult Post([FromBody] CreateItInterfaceRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creationParameters = _writeModelMapper.FromPOST(request);

            return _writeService
                .Create(request.OrganizationUuid, creationParameters)
                .Select(ToItInterfaceResponseDTO)
                .Match(MapItInterfaceCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Allows partial updates of an existing it-interface using json merge patch semantics (RFC7396)
        /// </summary>
        /// <param name="uuid">UUID of the interface in KITOS</param>
        /// <param name="request">Updates for the interface</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Patch([NonEmptyGuid] Guid uuid, [FromBody] UpdateItInterfaceRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = _writeModelMapper.FromPATCH(request);

            return _writeService
                .Update(uuid, updateParameters)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }

        //TODO: Get deletion conflicts? - add story to add the endpoint - better than trial and error for the user

        /// <summary>
        /// Delete an It-interface
        /// Constraints:
        /// - Exposing it-system must be reset before deleting this it-interface
        /// </summary>
        /// <param name="uuid">UUID of the interface in KITOS</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Patch([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Delete(uuid)
                .Match(NoContent, FromOperationError);
        }

        /// <summary>
        /// Creates a new IT-Interface based on given input values
        /// </summary>
        /// <param name="request">A collection of specific IT-Interface values</param>
        /// <returns>Location header is set to uri for newly created IT-Interface</returns>
        [HttpPost]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(RightsHolderItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult PostItInterfaceAsRightsHolder([FromBody] RightsHolderCreateItInterfaceRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creationParameters = _writeModelMapper.FromPOST(request);

            return _rightsHolderService
                .CreateNewItInterface(request.RightsHolderUuid, creationParameters)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(MapItInterfaceCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Returns all IT-Interfaces for which the user has rights holders access
        /// </summary>
        /// <param name="rightsHolderUuid">Uuid of the organization you want interfaces from. If not provided all available interfaces (based on access rights) will be returned</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-interfaces</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <returns></returns>
        [HttpGet]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RightsHolderItInterfaceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItInterfacesAsRightsHolder(
            [NonEmptyGuid] Guid? rightsHolderUuid = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            [FromUri] BoundedPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItInterface>>();

            if (includeDeactivated != true)
                refinements.Add(new QueryByEnabledEntitiesOnly<ItInterface>());

            if (changedSinceGtEq.HasValue)
                refinements.Add(new QueryByChangedSinceGtEq<ItInterface>(changedSinceGtEq.Value));

            return _rightsHolderService
                .GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(refinements, rightsHolderUuid)
                .Match(
                    success => success
                        .OrderByDefaultConventions(changedSinceGtEq.HasValue)
                        .Page(pagination)
                        .ToList()
                        .Select(ToRightsHolderItInterfaceResponseDTO)
                        .Transform(Ok),
                    FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-Interface
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <returns>Specific data related to the IT-Interface</returns>
        [HttpGet]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid)
        {
            return _rightsHolderService
                .GetInterfaceAsRightsHolder(uuid)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Sets IT-Interface values
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <returns>The updated IT-Interface</returns>
        [HttpPut]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] RightsHolderWritableItInterfacePropertiesDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = _writeModelMapper.FromPUT(request);

            return _rightsHolderService
                .UpdateItInterface(uuid, updateParameters)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }


        /// <summary>
        /// Allows partial updates of an existing it-interface using json merge patch semantics (RFC7396)
        /// </summary>
        /// <param name="uuid">UUID of the interface in KITOS</param>
        /// <param name="request">Updates for the interface</param>
        /// <returns></returns>
        [HttpPatch]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PatchItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] RightsHolderPartialUpdateItInterfaceRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = _writeModelMapper.FromPATCH(request);

            return _rightsHolderService
                .UpdateItInterface(uuid, updateParameters)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deactivates an IT-Interface
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <param name="deactivationReasonDTO">Reason for deactivation</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeactivateItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] DeactivationReasonRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHolderService
                .Deactivate(uuid, request.DeactivationReason)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(_ => StatusCode(HttpStatusCode.NoContent), FromOperationError);
        }

        /// <summary>
        /// Returns IT-Interfaces available to the user
        /// </summary>
        /// <param name="exposedBySystemUuid">IT-System UUID filter</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-interfaces</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-interfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItInterfaceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItInterfaces(
            [NonEmptyGuid] Guid? exposedBySystemUuid = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            [FromUri] BoundedPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItInterface>>();

            if (exposedBySystemUuid.HasValue)
                refinements.Add(new QueryByExposingSystem(exposedBySystemUuid.Value));

            if (includeDeactivated != true)
                refinements.Add(new QueryByEnabledEntitiesOnly<ItInterface>());

            if (changedSinceGtEq.HasValue)
                refinements.Add(new QueryByChangedSinceGtEq<ItInterface>(changedSinceGtEq.Value));

            return _itInterfaceService
                .GetAvailableInterfaces(refinements.ToArray())
                .OrderByDefaultConventions(changedSinceGtEq.HasValue)
                .Page(pagination)
                .ToList()
                .Select(ToItInterfaceResponseDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns requested IT-Interface
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <returns>Specific data related to the IT-Interface</returns>
        [HttpGet]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItInterface([NonEmptyGuid] Guid uuid)
        {
            return _itInterfaceService
                .GetInterface(uuid)
                .Select(ToItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }

        private RightsHolderItInterfaceResponseDTO ToRightsHolderItInterfaceResponseDTO(ItInterface itInterface)
        {
            return _responseMapper.ToRightsHolderItInterfaceResponseDTO(itInterface);
          
        }

        private ItInterfaceResponseDTO ToItInterfaceResponseDTO(ItInterface itInterface)
        {
            return _responseMapper.ToItInterfaceResponseDTO(itInterface);
        }

        private CreatedNegotiatedContentResult<T> MapItInterfaceCreatedResponse<T>(T dto) where T: IHasUuidExternal
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}