using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [PublicApi]
    public class ItContractsController : BaseEntityController<ItContract>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IOrganizationUnitRepository _organizationUnitRepository;

        public ItContractsController(
            IGenericRepository<ItContract> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IOrganizationUnitRepository organizationUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
            _organizationUnitRepository = organizationUnitRepository;
        }

        /// <summary>
        /// Hvis den autentificerede bruger er Global Admin, returneres alle kontrakter.
        /// Ellers returneres de kontrakter som brugeren har rettigheder til at se.
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter alle organisationens IT Kontrakter
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItContracts(int key)
        {
            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(key);
            if (organizationDataReadAccessLevel != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable().ByOrganizationId(key);

            return Ok(result);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<List<ItContract>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItContractsByOrgUnit(int orgKey, int unitKey)
        {
            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(orgKey);
            if (organizationDataReadAccessLevel < OrganizationDataReadAccessLevel.Public)
            {
                return Forbidden();
            }

            var orgUnitTreeIds = _organizationUnitRepository.GetIdsOfSubTree(orgKey, unitKey).ToList();

            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey)
                .Where(usage => usage.ResponsibleOrganizationUnitId != null && orgUnitTreeIds.Contains(usage.ResponsibleOrganizationUnitId.Value));

            return Ok(result);
        }
    }
}
