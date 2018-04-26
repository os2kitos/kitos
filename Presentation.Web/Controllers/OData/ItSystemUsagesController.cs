using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System.Net;
using Core.DomainModel.Organization;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemUsagesController : BaseEntityController<ItSystemUsage>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<AccessType> _accessTypeRepository;
        private readonly IAuthenticationService _authService;

        public ItSystemUsagesController(IGenericRepository<ItSystemUsage> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, IAuthenticationService authService, IGenericRepository<AccessType> accessTypeRepository )
            : base(repository, authService)
        {
            _orgUnitRepository = orgUnitRepository;
            _accessTypeRepository = accessTypeRepository;
            _authService = authService;
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 3 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({key})/ItSystemUsages")]
        public IHttpActionResult GetItSystems(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != key && !_authService.HasReadAccessOutsideContext(UserId))
                return StatusCode(HttpStatusCode.Forbidden);
            //Tolist() is required for filtering on computed values in odata.
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key).ToList();
            return Ok(result);
        }

        // TODO refactor this now that we are using MS Sql Server that has support for MARS
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 3 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItSystemUsages")]
        public IHttpActionResult GetItSystemsByOrgUnit(int orgKey, int unitKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
                return StatusCode(HttpStatusCode.Forbidden);

            var systemUsages = new List<ItSystemUsage>();

            // using iteration instead of recursion else we're running into
            // an "multiple DataReaders open" issue and MySQL doesn't support MARS

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
        public IHttpActionResult CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            var itSystemUsage = Repository.GetByKey(key);
            if (itSystemUsage == null)
            {
                return NotFound();
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
                return StatusCode(HttpStatusCode.NotFound);
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
