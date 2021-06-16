using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2")]
    public class ItInterfaceV2Controller: ExternalBaseController
    {
        private readonly IRightsHoldersService _rightsHolderService;
        private readonly IItInterfaceService _itInterfaceService;

        public ItInterfaceV2Controller(IRightsHoldersService rightsHolderService, IItInterfaceService itInterfaceService)
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
        public IHttpActionResult PostItInterface([FromBody] ItInterfaceRequestDTO itInterfaceDTO)
        {
            return Created(Request.RequestUri + "/" + itInterfaceDTO.Uuid, new ItInterfaceResponseDTO());
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItInterfaceResponseDTO>))]
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
                        .Select(ToDTO)
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItInterface(Guid uuid)
        {
            return _rightsHolderService
                .GetInterfaceAsRightsHolder(uuid)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Sets IT-Interface values
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <returns>The updated IT-Interface</returns>
        [HttpPut]
        [Route("rightsholder/it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItInterface(Guid uuid, [FromBody] ItInterfaceRequestDTO itInterfaceRequestDTO)
        {
            return Ok(new ItInterfaceResponseDTO());
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
                .Select(ToDTO)
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
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        private ItInterfaceResponseDTO ToDTO(ItInterface input)
        {
            //TODO: JMO use new IdNamePair mapper after merge
            return new ItInterfaceResponseDTO()
            {
                Uuid = input.Uuid,
                ExposedBySystem = new IdentityNamePairResponseDTO(input.ExhibitedBy.ItSystem.Uuid, input.ExhibitedBy.ItSystem.Name),
                Name = input.Name,
                InterfaceId = input.ItInterfaceId,
                Version = input.Version,
                Description = input.Description,
                UrlReference = input.Url,
                Deactivated = input.Disabled,
                Created = input.Created,
                CreatedBy = new IdentityNamePairResponseDTO(input.ObjectOwner.Uuid, input.ObjectOwner.GetFullName())
            };
        }
    }
}