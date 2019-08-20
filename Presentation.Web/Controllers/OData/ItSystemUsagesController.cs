using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Net;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainModel.ItSystem;
using Core.ApplicationServices;
using Ninject.Infrastructure.Language;
using Presentation.Web.Access;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItSystemUsagesController : BaseEntityController<ItSystemUsage>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<AccessType> _accessTypeRepository;
        private readonly IOrganizationContextFactory _contextFactory;

        public ItSystemUsagesController(IGenericRepository<ItSystemUsage> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, 
            IAuthenticationService authService, IGenericRepository<AccessType> accessTypeRepository, IAccessContext accessContext)
            : base(repository, authService, accessContext)
        {
            _orgUnitRepository = orgUnitRepository;
            _accessTypeRepository = accessTypeRepository;
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 4 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({orgKey})/ItSystemUsages")]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            if (!AllowOrganizationAccess(orgKey))
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable(readOnly:true).Where(m => m.OrganizationId == orgKey);

            var itSystemUsages = result.ToEnumerable().Where(AllowReadAccess);

            return Ok(itSystemUsages);
        }

        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 4 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItSystemUsages")]
        public IHttpActionResult GetItSystemsByOrgUnit(int orgKey, int unitKey)
        {
            if (!AllowOrganizationAccess(orgKey))
            {
                return Forbidden();
            }

            var systemUsages = new List<ItSystemUsage>();
            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                var orgUnit = _orgUnitRepository.AsQueryable(readOnly:true)
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

            var result = systemUsages.Where(AllowReadAccess);

            return Ok(result);
        }

        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateRef([FromODataUri] int systemUsageKey, string navigationProperty, [FromBody] Uri link)
        {
            var itSystemUsage = Repository.GetByKey(systemUsageKey);
            if (itSystemUsage == null)
            {
                return NotFound();
            }

            if (!AllowWriteAccess(itSystemUsage))
            {
                return Forbidden();
            }
            
            switch (navigationProperty)
            {
                case "AccessTypes":
                    var relatedKey = GetKeyFromUri<int>(Request, link);
                    var accessType = _accessTypeRepository.GetByKey(relatedKey);
                    if (accessType == null)
                    {
                        return NotFound();
                    }

                    itSystemUsage.AccessTypes.Add(accessType);
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            Repository.Save();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] string relatedKey, string navigationProperty)
        {
            var itSystemUsage = Repository.GetByKey(key);
            if (itSystemUsage == null)
            {
                return NotFound();
            }

            if (!AllowWriteAccess(itSystemUsage))
            {
                return Forbidden();
            }

            switch (navigationProperty)
            {
                case "AccessTypes":
                    var accessTypeId = Convert.ToInt32(relatedKey);
                    var accessType = _accessTypeRepository.GetByKey(accessTypeId);

                    if (accessType == null)
                    {
                        return NotFound();
                    }
                    itSystemUsage.AccessTypes.Remove(accessType);
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);

            }

            Repository.Save();

            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}
