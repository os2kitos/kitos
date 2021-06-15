using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2")]
    public class GeneralStakeholderController: ExternalBaseController
    {
        private readonly IItInterfaceService _itInterfaceService;

        public GeneralStakeholderController(IItInterfaceService itInterfaceService)
        {
            _itInterfaceService = itInterfaceService;
        }


        /// <summary>
        /// Returns public and active IT-Systems
        /// </summary>
        /// <param name="rightsholderUuid">Rightsholder UUID filter</param>
        /// <param name="businessTypeUuid">Business type UUID filter</param>
        /// <param name="kleNumber">KLE number filter ("NN.NN.NN" format)</param>
        /// <param name="kleUuid">KLE UUID number filter</param>
        /// <param name="numberOfUsers">Greater than or equal to number of users filter</param>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemStakeholderResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(Guid? rightsholderUuid, Guid? businessTypeUuid, string? kleNumber, Guid? kleUuid, int? numberOfUsers, int? page = 0, int? pageSize = 100)
        {
            return Ok(new List<ItSystemStakeholderResponseDTO>());
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemStakeholderResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystem(Guid uuid)
        {
            return Ok(new ItSystemStakeholderResponseDTO());
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
        public IHttpActionResult GetItInterface(Guid? exposedBySystemUuid = null, [FromUri] StandardPaginationQuery pagination = null)
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
        public IHttpActionResult GetItInterface(Guid uuid)
        {
            return Ok(new ItInterfaceResponseDTO());
        }

        private ItInterfaceResponseDTO ToDTO(ItInterface input)
        {
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
                CreatedBy = new IdentityNamePairResponseDTO(input.ObjectOwner.Uuid, input.ObjectOwner.GetFullName()),
                LastModifiedBy = new IdentityNamePairResponseDTO(input.LastChangedByUser.Uuid, input.LastChangedByUser.GetFullName())
            };
        }
    }
}