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

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemUsagesController : BaseEntityController<ItSystemUsage>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IAuthenticationService _authService;

        public ItSystemUsagesController(IGenericRepository<ItSystemUsage> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _orgUnitRepository = orgUnitRepository;
            _authService = authService;
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery(MaxExpansionDepth = 3)] // MaxExpansionDepth is 3 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({key})/ItSystemUsages")]
        public IHttpActionResult GetItSystems(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != key && !_authService.HasReadAccessOutsideContext(UserId))
                return StatusCode(HttpStatusCode.Forbidden);

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }

        // TODO refactor this now that we are using MS Sql Server that has support for MARS
        [EnableQuery(MaxExpansionDepth = 3)] // MaxExpansionDepth is 3 because we need to do MainContract($expand=ItContract($expand=Supplier))
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
    }
}
