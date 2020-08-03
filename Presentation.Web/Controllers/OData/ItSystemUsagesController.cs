using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Net;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItSystemUsagesController : BaseEntityController<ItSystemUsage>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<AccessType> _accessTypeRepository;

        public ItSystemUsagesController(
            IGenericRepository<ItSystemUsage> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<AccessType> accessTypeRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
            _accessTypeRepository = accessTypeRepository;
        }

        /// <summary>
        /// Henter alle organisationens IT-Systemanvendelser.
        /// </summary>
        /// <param name="orgKey"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 4 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({orgKey})/ItSystemUsages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystemUsage>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            //Usages are local so full access is required
            var accessLevel = GetOrganizationReadAccessLevel(orgKey);
            if (accessLevel < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey);

            return Ok(result);
        }

        /// <summary>
        /// Henter alle IT-Systemanvendelser for den pågældende organisationsenhed
        /// </summary>
        /// <param name="orgKey"></param>
        /// <param name="unitKey"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 4 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItSystemUsages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystemUsage>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItSystemsByOrgUnit(int orgKey, int unitKey)
        {
            //Usages are local so full access is required
            if (GetOrganizationReadAccessLevel(orgKey) != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var systemUsages = new List<ItSystemUsage>();
            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                var orgUnit = _orgUnitRepository.AsQueryable()
                    .Include(x => x.Children)
                    .Include(x => x.Using.Select(y => y.ResponsibleItSystemUsage))
                    .First(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);
                var responsible =
                    orgUnit.Using.Select(x => x.ResponsibleItSystemUsage).Where(x => x != null).ToList();
                systemUsages.AddRange(responsible);
                var childIds = orgUnit.Children.Select(x => x.Id);
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return Ok(systemUsages);
        }

        [AcceptVerbs("POST", "PUT")]
        [ODataRoute("ItSystemUsages({key})/AccessTypes/{accessTypeId}")]
        public IHttpActionResult CreateRef(int key, int accessTypeId)
        {
            var itSystemUsage = Repository.GetByKey(key);
            if (itSystemUsage == null)
            {
                return NotFound();
            }

            if (!AllowModify(itSystemUsage))
            {
                return Forbidden();
            }

            var accessType = _accessTypeRepository.GetByKey(accessTypeId);
            if (accessType == null)
            {
                return BadRequest("Invalid accessTypeId");
            }

            itSystemUsage.AccessTypes.Add(accessType);

            Repository.Save();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("DELETE")]
        [ODataRoute("ItSystemUsages({key})/AccessTypes/{accessTypeId}")]
        public IHttpActionResult DeleteRef(int key, int accessTypeId)
        {
            var itSystemUsage = Repository.GetByKey(key);
            if (itSystemUsage == null)
            {
                return NotFound();
            }

            if (!AllowModify(itSystemUsage))
            {
                return Forbidden();
            }

            var accessType = _accessTypeRepository.GetByKey(accessTypeId);

            if (accessType == null)
            {
                return BadRequest("Invalid accessTypeId");
            }

            itSystemUsage.AccessTypes.Remove(accessType);

            Repository.Save();

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
