using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Presentation.Web.Models.External.V2.Response.Interface;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2")]
    public class ItInterfaceV2Controller: ExternalBaseController
    {
        private readonly IItInterfaceRightsHolderService _rightsHolderService;
        private readonly IItInterfaceService _itInterfaceService;

        public ItInterfaceV2Controller(IItInterfaceRightsHolderService rightsHolderService, IItInterfaceService itInterfaceService)
        {
            _rightsHolderService = rightsHolderService;
            _itInterfaceService = itInterfaceService;
        }


        /// <summary>
        /// Creates a new IT-Interface based on given input values
        /// </summary>
        /// <param name="itInterfaceDTO">A collection of specific IT-Interface values</param>
        /// <returns>Location header is set to uri for newly created IT-Interface</returns>
        [HttpPost]
        [Route("rightsholder/it-interfaces")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult PostItInterface([FromBody] RightsHolderCreateItInterfaceRequestDTO itInterfaceDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creationParameters = new RightsHolderItInterfaceCreationParameters(
                itInterfaceDTO.Uuid,
                itInterfaceDTO.ExposedBySystemUuid,
                itInterfaceDTO.Name, 
                itInterfaceDTO.InterfaceId, 
                itInterfaceDTO.Version, 
                itInterfaceDTO.Description, 
                itInterfaceDTO.UrlReference);

            return _rightsHolderService
                .CreateNewItInterface(itInterfaceDTO.RightsHolderUuid, creationParameters)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(MapItInterfaceCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Returns active IT-Interfaces
        /// </summary>
        /// <param name="organizationUuid">Uuid of the organization you want interfaces from</param>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("rightsholder/it-interfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RightsHolderItInterfaceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItInterface(Guid? rightsHolderUuid = null, [FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            return _rightsHolderService
                .GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(rightsHolderUuid)
                .Match(
                    success => success
                        .OrderBy(y => y.Id)
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
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItInterface(Guid uuid)
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
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItInterface(Guid uuid, [FromBody] RightsHolderWritableItInterfacePropertiesDTO itInterfaceDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = new RightsHolderItInterfaceUpdateParameters(
                itInterfaceDTO.ExposedBySystemUuid,
                itInterfaceDTO.Name,
                itInterfaceDTO.InterfaceId,
                itInterfaceDTO.Version,
                itInterfaceDTO.Description,
                itInterfaceDTO.UrlReference);

            return _rightsHolderService
                .UpdateItInterface(uuid, updateParameters)
                .Select(ToRightsHolderItInterfaceResponseDTO)
                .Match(MapItInterfaceCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Deactivates an IT-Interface
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <param name="deactivationReasonDTO">Reason for deactivation</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteItInterface(Guid uuid, [FromBody] DeactivationReasonRequestDTO deactivationReasonDTO)
        {
            return Ok();
        }

        /// <summary>
        /// Returns active IT-Interfaces
        /// </summary>
        /// <param name="exposedBySystemUuid">IT-System UUID filter</param>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-interfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItInterfaceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItInterfaceAsStakeholder(Guid? exposedBySystemUuid = null, [FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItInterface>>();

            if (exposedBySystemUuid.HasValue)
                refinements.Add(new QueryByExposingSystem(exposedBySystemUuid.Value));

            return _itInterfaceService
                .GetAvailableInterfaces(refinements.ToArray())
                .OrderBy(y => y.Id)
                .Page(pagination)
                .ToList()
                .Select(ToStakeHolderItInterfaceResponseDTO)
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
        public IHttpActionResult GetItInterfaceAsStakeholder(Guid uuid)
        {
            return _itInterfaceService
                .GetInterface(uuid)
                .Select(ToStakeHolderItInterfaceResponseDTO)
                .Match(Ok, FromOperationError);
        }

        private static RightsHolderItInterfaceResponseDTO ToRightsHolderItInterfaceResponseDTO(ItInterface itInterface)
        {
            var dto = new RightsHolderItInterfaceResponseDTO();
            MapBaseInformation(itInterface, dto);
            return dto;
        }

        private static ItInterfaceResponseDTO ToStakeHolderItInterfaceResponseDTO(ItInterface itInterface)
        {
            var dto = new ItInterfaceResponseDTO
            {
                LastModified = itInterface.LastChanged,
                LastModifiedBy = itInterface.LastChangedByUser.Transform(user => user.MapIdentityNamePairDTO())
            };
            MapBaseInformation(itInterface, dto);
            return dto;
        }

        private static void MapBaseInformation<T>(ItInterface input, T outputDTO) where T: BaseItInterfaceResponseDTO
        {
            outputDTO.Uuid = input.Uuid;
            outputDTO.ExposedBySystem = input.ExhibitedBy?.ItSystem?.Transform(exposingSystem => exposingSystem.MapIdentityNamePairDTO());
            outputDTO.Name = input.Name;
            outputDTO.InterfaceId = input.ItInterfaceId;
            outputDTO.Version = input.Version;
            outputDTO.Description = input.Description;
            outputDTO.UrlReference = input.Url;
            outputDTO.Deactivated = input.Disabled;
            outputDTO.Created = input.Created;
            outputDTO.CreatedBy = input.ObjectOwner.MapIdentityNamePairDTO();
        }

        private CreatedNegotiatedContentResult<RightsHolderItInterfaceResponseDTO> MapItInterfaceCreatedResponse(RightsHolderItInterfaceResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}