using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Interface.Write;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Response.Interface;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces
{
    [Route("api/v2")]
    public class ItInterfaceV2Controller : ExternalBaseController
    {
        private readonly IItInterfaceRightsHolderService _rightsHolderService;
        private readonly IItInterfaceService _itInterfaceService;
        private readonly IItInterfaceWriteModelMapper _writeModelMapper;
        private readonly IItInterfaceWriteService _writeService;
        private readonly IItInterfaceResponseMapper _responseMapper;
        private readonly IResourcePermissionsResponseMapper _permissionResponseMapper;

        public ItInterfaceV2Controller(
            IItInterfaceRightsHolderService rightsHolderService,
            IItInterfaceService itInterfaceService,
            IItInterfaceWriteModelMapper writeModelMapper,
            IItInterfaceWriteService writeService,
            IItInterfaceResponseMapper responseMapper,
            IResourcePermissionsResponseMapper permissionResponseMapper)
        {
            _rightsHolderService = rightsHolderService;
            _itInterfaceService = itInterfaceService;
            _writeModelMapper = writeModelMapper;
            _writeService = writeService;
            _responseMapper = responseMapper;
            _permissionResponseMapper = permissionResponseMapper;
        }

        /// <summary>
        /// Creates a new IT-Interface based on given input values
        /// </summary>
        /// <param name="request">A collection of specific IT-Interface values</param>
        /// <returns>Location header is set to uri for newly created IT-Interface</returns>
        [HttpPost]
        [Route("it-interfaces")]
        [ApiResponse(typeof(ItInterfaceResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Conflict)]
        public IActionResult Post([FromBody] CreateItInterfaceRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creationParameters = _writeModelMapper.FromPOST(request);

            return _writeService
                .Create(request.OrganizationUuid, creationParameters)
                .Select(ToItInterfaceResponseDTO)
                .Match(ToCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Allows partial updates of an existing it-interface using json merge patch semantics (RFC7396)
        /// </summary>
        /// <param name="uuid">UUID of the interface in KITOS</param>
        /// <param name="request">Updates for the interface</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("it-interfaces/{uuid}")]
        [ApiResponse(typeof(ItInterfaceResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Patch([NonEmptyGuid] Guid uuid, [FromBody] UpdateItInterfaceRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = _writeModelMapper.FromPATCH(request);

            return _writeService
                .Update(uuid, updateParameters)
                .Select(ToItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Delete an It-interface
        /// Constraints:
        /// - Exposing it-system must be reset before deleting this it-interface
        /// </summary>
        /// <param name="uuid">UUID of the interface in KITOS</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("it-interfaces/{uuid}")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Delete([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Delete(uuid)
                .Match(NoContent, FromOperationError);
        }

        /// <summary>
        /// Creates a new IT-Interface data description
        /// </summary>
        /// <param name="request">A collection of specific IT-Interface data description values</param>
        /// <param name="uuid">UUID of the IT-Interface in KITOS</param>
        /// <returns>Location header is set to uri for newly created IT-Interface data description</returns>
        [HttpPost]
        [Route("it-interfaces/{uuid}/data")]
        [ApiResponse(typeof(ItInterfaceDataResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PostDataDescription([NonEmptyGuid] Guid uuid, [FromBody] ItInterfaceDataRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.MapDataDescription(request);

            return _writeService
                .AddDataDescription(uuid, parameters)
                .Select(_responseMapper.ToDataResponseDTO)
                .Match(ToCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Replace an existing IT-Interface data description
        /// </summary>
        /// <param name="request">A collection of specific IT-Interface data description values</param>
        /// <param name="uuid">UUID of the IT-Interface in KITOS</param>
        /// <param name="dataDescriptionUuid">UUID of the data description in KITOS</param>
        /// <returns>Updated data description</returns>
        [HttpPut]
        [Route("it-interfaces/{uuid}/data/{dataDescriptionUuid}")]
        [ApiResponse(typeof(ItInterfaceDataResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PutDataDescription([NonEmptyGuid] Guid uuid, [NonEmptyGuid] Guid dataDescriptionUuid, [FromBody] ItInterfaceDataRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.MapDataDescription(request);

            return _writeService
                .UpdateDataDescription(uuid, dataDescriptionUuid, parameters)
                .Select(_responseMapper.ToDataResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Delete an existing IT-Interface data description
        /// </summary>
        /// <param name="uuid">UUID of the IT-Interface in KITOS</param>
        /// <param name="dataDescriptionUuid">UUID of the data description in KITOS</param>
        /// <returns>Updated data description</returns>
        [HttpDelete]
        [Route("it-interfaces/{uuid}/data/{dataDescriptionUuid}")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteDataDescription([NonEmptyGuid] Guid uuid, [NonEmptyGuid] Guid dataDescriptionUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteDataDescription(uuid, dataDescriptionUuid)
                .Match(FromOperationError, NoContent);
        }

        /// <summary>
        /// Creates a new IT-Interface based on given input values
        /// </summary>
        /// <param name="request">A collection of specific IT-Interface values</param>
        /// <returns>Location header is set to uri for newly created IT-Interface</returns>
        [HttpPost]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces")]
        [ApiResponse(typeof(RightsHolderItInterfaceResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Conflict)]
        public IActionResult PostItInterfaceAsRightsHolder([FromBody] RightsHolderCreateItInterfaceRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creationParameters = _writeModelMapper.FromPOST(request);

            return _rightsHolderService
                .CreateNewItInterface(request.RightsHolderUuid, creationParameters)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(ToCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Returns all IT-Interfaces for which the user has rights holders access
        /// </summary>
        /// <param name="rightsHolderUuid">Uuid of the organization you want interfaces from. If not provided all available interfaces (based on access rights) will be returned</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-interfaces</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <param name="pagination">Pagination parameters</param>
        /// <returns></returns>
        [HttpGet]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces")]
        [ApiResponse(typeof(IEnumerable<RightsHolderItInterfaceResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetItInterfacesAsRightsHolder(
            [NonEmptyGuid] Guid? rightsHolderUuid = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            [FromQuery] BoundedPaginationQuery? pagination = null)
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
                        .OrderApiResultsByDefaultConventions(changedSinceGtEq.HasValue)
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
        [ApiResponse(typeof(RightsHolderItInterfaceResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid)
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
        /// <param name="request">Updated IT-Interface properties</param>
        /// <returns>The updated IT-Interface</returns>
        [HttpPut]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [ApiResponse(typeof(RightsHolderItInterfaceResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult PutItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] RightsHolderWritableItInterfacePropertiesDTO request)
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
        [ApiResponse(typeof(RightsHolderItInterfaceResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult PatchItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] RightsHolderPartialUpdateItInterfaceRequestDTO request)
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
        /// <param name="request">Reason for deactivation</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [AllowRightsHoldersAccess]
        [Consumes("application/json")]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult DeactivateItInterfaceAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] DeactivationReasonRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHolderService
                .Deactivate(uuid, request.DeactivationReason)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(_ => StatusCode((int)HttpStatusCode.NoContent), FromOperationError);
        }

        /// <summary>
        /// Returns IT-Interfaces available to the user
        /// </summary>
        /// <param name="exposedBySystemUuid">IT-System UUID filter</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-interfaces</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <param name="nameEquals">Include only interfaces with a name equal to the parameter</param>
        /// <param name="usedInOrganizationUuid">Filter by UUID of an organization which has taken the related it-system into use through an it-system-usage resource</param>
        /// <param name="nameContains">Filter by contents of the name</param>
        /// <param name="interfaceId">Include only interfaces with an InterfaceId equal to the parameter</param>
        /// <param name="organizationUuid">Query it-interfaces created in a specific organization</param>
        /// <param name="orderByProperty">Ordering property</param>
        /// <param name="availableInOrganizationUuid">Filter by UUID of an organization which has access to the IT-Interface</param>
        /// <param name="nameOrItInterfaceIdContains">Filter by contents of the name or IT-Interface ID</param>
        /// <param name="pagination">Pagination parameters</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-interfaces")]
        [ApiResponse(typeof(IEnumerable<ItInterfaceResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetItInterfaces(
            [NonEmptyGuid] Guid? exposedBySystemUuid = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            string? nameEquals = null,
            [NonEmptyGuid] Guid? usedInOrganizationUuid = null,
            string? nameContains = null,
            string? interfaceId = null,
            [NonEmptyGuid] Guid? organizationUuid = null,
            CommonOrderByProperty? orderByProperty = null,
            [NonEmptyGuid] Guid? availableInOrganizationUuid = null,
            string? nameOrItInterfaceIdContains = null,
            [FromQuery] BoundedPaginationQuery? pagination = null)
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

            if (nameEquals != null)
                refinements.Add(new QueryByName<ItInterface>(nameEquals));

            if (usedInOrganizationUuid.HasValue)
                refinements.Add(new QueryInterfaceByUsedInOrganizationWithUuid(usedInOrganizationUuid.Value));

            if (nameContains != null)
                refinements.Add(new QueryByPartOfName<ItInterface>(nameContains));

            if (interfaceId != null)
                refinements.Add(new QueryByInterfaceId(interfaceId));

            if (organizationUuid.HasValue)
                refinements.Add(new QueryByOrganizationUuid<ItInterface>(organizationUuid.Value));

            if (availableInOrganizationUuid.HasValue)
                refinements.Add(new QueryByPublicAccessOrOrganizationUuid<ItInterface>(availableInOrganizationUuid.Value));

            if (nameOrItInterfaceIdContains != null)
                refinements.Add(new QueryByNameOrItInterfaceId(nameOrItInterfaceIdContains));

            return _itInterfaceService
                .GetAvailableInterfaces(refinements.ToArray())
                .OrderApiResultsByDefaultConventions(changedSinceGtEq.HasValue, orderByProperty)
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
        [ApiResponse(typeof(ItInterfaceResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItInterface([NonEmptyGuid] Guid uuid)
        {
            return _itInterfaceService
                .GetInterface(uuid)
                .Select(ToItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the permissions of the authenticated client in the context of a specific IT-Interface
        /// </summary>
        /// <param name="interfaceUuid">UUID of the interface entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-interfaces/{interfaceUuid}/permissions")]
        [ApiResponse(typeof(ItInterfacePermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItInterfacePermissions([NonEmptyGuid] Guid interfaceUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itInterfaceService
                .GetPermissions(interfaceUuid)
                .Select(_responseMapper.Map)
                .Match(Ok, FromOperationError);
        }


        /// <summary>
        /// Returns the permissions of the authenticated client for the IT-Interface resources collection in the context of an organization (IT-Interface permissions in a specific Organization)
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-interfaces/permissions")]
        [ApiResponse(typeof(ResourceCollectionPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItInterfaceCollectionPermissions([Required][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itInterfaceService.GetCollectionPermissions(organizationUuid)
                .Select(_permissionResponseMapper.Map)
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

        private IActionResult ToCreatedResponse<T>(T dto) where T : IHasUuidExternal
        {
            return Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}


