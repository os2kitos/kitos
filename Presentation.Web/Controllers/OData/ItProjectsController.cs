using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [Authorize]
    [PublicApi]
    public class ItProjectsController : BaseEntityController<ItProject>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IOrganizationUnitRepository _organizationUnitRepository;

        public ItProjectsController(IGenericRepository<ItProject> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, IOrganizationUnitRepository organizationUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
            _organizationUnitRepository = organizationUnitRepository;
        }

        [EnableQuery]
        [ODataRoute("ItProjects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<ItProject>))]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter organisationens projekter samt offentlige projekter fra andre organisationer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItProjects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItProject>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItProjects(int key)
        {
            var all = Repository.AsQueryable();

            if (GetCrossOrganizationReadAccessLevel() < CrossOrganizationDataReadAccessLevel.All)
            {
                if (GetOrganizationReadAccessLevel(key) < OrganizationDataReadAccessLevel.All)
                {
                    return Forbidden();
                }

                var result = all.ByOrganizationId(key);
                return Ok(result);
            }
            else
            {
                var result = all
                    .ByPublicAccessOrOrganizationId(key);

                return Ok(result);
            }
        }

        // GET /Organizations(1)/OrganizationUnits(1)/ItProjects
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItProjects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<List<ItProject>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItProjectsByOrgUnit(int orgKey, int unitKey)
        {
            if (GetOrganizationReadAccessLevel(orgKey) < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var orgUnitTreeIds = _organizationUnitRepository.GetSubTree(orgKey, unitKey);

            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey)
                .Where(usage => usage.ResponsibleUsage != null && orgUnitTreeIds.Contains(usage.ResponsibleUsage.OrganizationUnitId));

            return Ok(result);
        }

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Post(int organizationId, ItProject entity) => throw new NotSupportedException();
    }
}
