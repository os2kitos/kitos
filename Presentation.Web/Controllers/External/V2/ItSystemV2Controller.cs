using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.ItSystem;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2")]
    public class ItSystemV2Controller : ExternalBaseController
    {
        private readonly IItSystemService _itSystemService;
        private readonly IRightsHoldersService _rightsHoldersService;

        public ItSystemV2Controller(IItSystemService itSystemService, IRightsHoldersService rightsHoldersService)
        {
            _itSystemService = itSystemService;
            _rightsHoldersService = rightsHoldersService;
        }

        /// <summary>
        /// Returns public and active IT-Systems
        /// </summary>
        /// <param name="rightsHolderUuid">Rightsholder UUID filter</param>
        /// <param name="businessTypeUuid">Business type UUID filter</param>
        /// <param name="kleNumber">KLE number filter ("NN.NN.NN" format)</param>
        /// <param name="kleUuid">KLE UUID number filter</param>
        /// <param name="numberOfUsers">Greater than or equal to number of users filter</param>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(
            Guid? rightsHolderUuid = null,
            Guid? businessTypeUuid = null,
            string kleNumber = null,
            Guid? kleUuid = null,
            int? numberOfUsers = null,
            [FromUri] StandardPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItSystem>>();

            if (rightsHolderUuid.HasValue)
                refinements.Add(new QueryByRightsHolderUuid(rightsHolderUuid.Value));

            if (businessTypeUuid.HasValue)
                refinements.Add(new QueryByBusinessType(businessTypeUuid.Value));

            if (kleNumber != null || kleUuid.HasValue)
                refinements.Add(new QueryByTaskRef(kleNumber, kleUuid));

            if (numberOfUsers.HasValue)
                refinements.Add(new QueryByNumberOfUsages(numberOfUsers.Value));

            return _itSystemService.GetAvailableSystems(refinements.ToArray())
                .OrderBy(x => x.Id)
                .Page(paginationQuery)
                .ToList()
                .Select(ToSystemResponseDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystem(Guid uuid)
        {
            if (uuid == Guid.Empty)
                return BadRequest(nameof(uuid) + " must be a non-empty guid");

            return _itSystemService
                .GetSystem(uuid)
                .Select(ToSystemResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns active IT-Systems
        /// </summary>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("rightsholder/it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemInformationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemsByRightsHoldersAccess([FromUri] StandardPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHoldersService
                .GetAvailableSystems()
                .Select(itSystems => itSystems
                    .OrderBy(system => system.Id)
                    .Page(paginationQuery)
                    .ToList()
                    .Select(ToSystemInformationResponseDTO)
                    .ToList())
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemInformationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemByRightsHoldersAccess(Guid uuid)
        {
            if (uuid == Guid.Empty)
                return BadRequest(nameof(uuid) + " must be a non-empty guid");

            return _rightsHoldersService
                .GetSystem(uuid)
                .Select(ToSystemInformationResponseDTO)
                .Match(Ok, FromOperationError);
        }

        private static ItSystemInformationResponseDTO ToSystemInformationResponseDTO(ItSystem itSystem)
        {
            var dto = new ItSystemInformationResponseDTO();
            MapBaseInformation(itSystem, dto);
            return dto;
        }

        private static ItSystemResponseDTO ToSystemResponseDTO(ItSystem itSystem)
        {
            var dto = new ItSystemResponseDTO
            {
                UsingOrganizations = itSystem
                    .Usages
                    .Select(systemUsage => systemUsage.Organization)
                    .Select(organization => organization.MapOrganizationResponseDTO())
                    .ToList()
            };

            MapBaseInformation(itSystem, dto);

            return dto;
        }

        private static void MapBaseInformation<T>(ItSystem arg, T dto) where T : ItSystemInformationResponseDTO
        {
            dto.Uuid = arg.Uuid;
            dto.Name = arg.Name;
            dto.RightsHolder = arg.BelongsTo?.Transform(organization => organization.MapOrganizationResponseDTO());
            dto.BusinessType = arg.BusinessType?.Transform(businessType => businessType.MapIdentityNamePairDTO());
            dto.Description = arg.Description;
            dto.CreatedBy = arg.ObjectOwner.MapIdentityNamePairDTO();
            dto.Created = arg.Created;
            dto.Deactivated = arg.Disabled;
            dto.FormerName = arg.PreviousName;
            dto.LastModified = arg.LastChanged;
            dto.LastModifiedBy = arg.LastChangedByUser.Transform(user => user.MapIdentityNamePairDTO());
            dto.ParentSystem = arg.Parent?.Transform(parent => parent.MapIdentityNamePairDTO());
            dto.UrlReference = arg.Reference?.URL;
            dto.ExposedInterfaces = arg
                .ItInterfaceExhibits
                .Select(exhibit => exhibit.ItInterface)
                .ToList()
                .Select(x => x.MapIdentityNamePairDTO())
                .ToList();
            dto.RecommendedArchiveDutyResponse =
                new RecommendedArchiveDutyResponseDTO(arg.ArchiveDutyComment, arg.ArchiveDuty.ToDTOType());
            dto.KLE = arg
                .TaskRefs
                .Select(taskRef => new IdentityNamePairResponseDTO(taskRef.Uuid, taskRef.TaskKey))
                .ToList();
        }
    }
}