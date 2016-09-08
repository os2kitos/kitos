using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using System.Web.Http.Results;
using System.Net;
using Core.DomainModel.Organization;

namespace Presentation.Web.Controllers.OData
{
    public class ItContractsController : BaseEntityController<ItContract>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public ItContractsController(IGenericRepository<ItContract> repository, IGenericRepository<OrganizationUnit> orgUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        [EnableQuery]
        [ODataRoute("ItContracts")]
        public override IHttpActionResult Get()
        {
            return base.Get();
            //if (AuthenticationService.HasReadAccessOutsideContext(CurentUser))
            //    return base.Get();

            //var orgId = CurrentOrganizationId;
            //return Ok(Repository.AsQueryable().Where(x => x.OrganizationId == orgId));
        }

        // GET /ItContracts(1)/ResponsibleOrganizationUnit
        [EnableQuery]
        [ODataRoute("ItContracts({contractKey})/ResponsibleOrganizationUnit")]
        public IHttpActionResult GetResponsibleOrganizationUnit(int contractKey)
        {
            var entity = Repository.GetByKey(contractKey).ResponsibleOrganizationUnit;
            if (entity == null)
                return NotFound();

            if (AuthenticationService.HasReadAccess(CurentUser, entity))
                return Ok(entity);

            return new StatusCodeResult(HttpStatusCode.Forbidden, this);
        }

        // GET /ItContracts(1)/ResponsibleOrganizationUnit
        [EnableQuery]
        [ODataRoute("ItContracts({contractKey})/Organization")]
        public IHttpActionResult GetOrganization(int contractKey)
        {
            var entity = Repository.GetByKey(contractKey).Organization;
            if (entity == null)
                return NotFound();

            if (AuthenticationService.HasReadAccess(CurentUser, entity))
                return Ok(entity);

            return new StatusCodeResult(HttpStatusCode.Forbidden, this);
        }

        // GET /Organizations(1)/ItContracts
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/ItContracts")]
        public IHttpActionResult GetItContracts(int key)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (loggedIntoOrgId != key && !AuthenticationService.HasReadAccessOutsideContext(CurentUser))
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }

        // GET /Organizations(1)/Supplier
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/Supplier")]
        public IHttpActionResult GetSupplier(int key)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (loggedIntoOrgId != key && !AuthenticationService.HasReadAccessOutsideContext(CurentUser))
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }

        // GET /Organizations(1)/ItContracts(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItContracts({contractKey})")]
        public IHttpActionResult GetItContracts(int orgKey, int contractKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == contractKey);
            if (entity == null)
                return NotFound();

            if (AuthenticationService.HasReadAccess(CurentUser, entity))
                return Ok(entity);

            return new StatusCodeResult(HttpStatusCode.Forbidden, this);
        }

        // TODO refactor this now that we are using MS Sql Server that has support for MARS
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItContracts")]
        public IHttpActionResult GetItContractsByOrgUnit(int orgKey, int unitKey)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (loggedIntoOrgId != orgKey && !AuthenticationService.HasReadAccessOutsideContext(CurentUser))
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

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
            return Ok(contracts);
        }
    }
}
