using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
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
        private readonly IAuthorizationContext _authorizationContext;

        public ItSystemV2Controller(IItSystemService itSystemService, IAuthorizationContext authorizationContext)
        {
            _itSystemService = itSystemService;
            _authorizationContext = authorizationContext;
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemInResponseDto>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(
            Guid? rightsHolderUuid = null,
            Guid? businessTypeUuid = null,
            string kleNumber = null,
            Guid? kleUuid = null,
            int? numberOfUsers = null,
            StandardPaginationQuery paginationQuery = null)
        {
            var refinements = new List<IDomainQuery<ItSystem>>();

            if (rightsHolderUuid.HasValue)
                refinements.Add(new QueryByRightsHolder(rightsHolderUuid.Value));

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
                .Select(ToDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemInResponseDto))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystem(Guid uuid)
        {
            return _itSystemService
                .GetSystem(uuid)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        private ItSystemInResponseDto ToDTO(ItSystem arg)
        {
            return new()
            {
                Uuid = arg.Uuid,
                Name = arg.Name,
                RightsHolder = arg.BelongsTo?.Transform(organization => organization.MapOrganizationResponseDTO()),
                BusinessType = arg.BusinessType?.Transform(businessType => businessType.MapIdentityNamePairDTO()),
                Description = arg.Description,
                CreatedBy = arg.ObjectOwner.MapIdentityNamePairDTO(),
                Created = arg.Created,
                Deactivated = arg.Disabled,
                FormerName = arg.PreviousName,
                LastModified = arg.LastChanged,
                LastModifiedBy = arg.LastChangedByUser.Transform(user => user.MapIdentityNamePairDTO()),
                ParentSystem = arg.Parent?.Transform(parent => parent.MapIdentityNamePairDTO()),
                UrlReference = arg.Reference?.URL,
                ExposedInterfaces = arg
                    .ItInterfaceExhibits
                    .Select(exhibit => exhibit.ItInterface)
                    .ToList()
                    .Where(_authorizationContext.AllowReads)//Some interfaces descriptions may not be read-accessible (created as private in an organization which the user is not a member of).
                    .Select(x => x.MapIdentityNamePairDTO())
                    .ToList(),
                RecommendedArchiveDutyResponse = new RecommendedArchiveDutyResponseDTO(arg.ArchiveDutyComment, arg.ArchiveDuty.ToDTOType()),
                UsingOrganizations = arg
                    .Usages
                    .Select(systemUsage => systemUsage.Organization)
                    .Select(organization => organization.MapOrganizationResponseDTO())
                    .ToList(),
                KLE = arg
                    .TaskRefs
                    .Select(taskRef => new IdentityNamePairResponseDTO(taskRef.Uuid, taskRef.TaskKey))
                    .ToList()
            };
        }
    }
}