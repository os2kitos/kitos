using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItContractsController : BaseEntityController<ItContract>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public ItContractsController(
            IGenericRepository<ItContract> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        /// <summary>
        /// Hvis den autentificerede bruger er Global Admin, returneres alle kontrakter.
        /// Ellers returneres organisationens kontrakter.
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        public override IHttpActionResult Get()
        {
            if (GetCrossOrganizationReadAccessLevel() == CrossOrganizationDataReadAccessLevel.All)
            {
                return Ok(Repository.AsQueryable());
            }

            var activeOrganizationId = UserContext.ActiveOrganizationId;
            return Ok(Repository.AsQueryable().ByOrganizationId(activeOrganizationId));
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
        public IHttpActionResult GetItContracts(int key)
        {
            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(key);
            if (organizationDataReadAccessLevel != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            //tolist requried to handle filtering on computed fields
            var result = Repository.AsQueryable().ByOrganizationId(key);

            return Ok(result);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<List<ItContract>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItContractsByOrgUnit(int orgKey, int unitKey)
        {
            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(orgKey);
            if (organizationDataReadAccessLevel < OrganizationDataReadAccessLevel.Public)
            {
                return Forbidden();
            }

            var contracts = new List<ItContract>();

            // using iteration instead of recursion else we're running into
            // an "multiple DataReaders open" issue and MySQL doesn't support MARS

            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                var orgUnit = _orgUnitRepository.AsQueryable()
                    .Include(x => x.Children)
                    .Include(x => x.ResponsibleForItContracts)
                    .FirstOrDefault(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);

                if (orgUnit != null)
                {
                    contracts.AddRange(orgUnit.ResponsibleForItContracts);

                    var childIds = orgUnit.Children.Select(x => x.Id);
                    foreach (var childId in childIds)
                        queue.Enqueue(childId);
                }

            }
            return Ok(contracts.Where(AllowRead));
        }
    }
}
