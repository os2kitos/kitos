using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItContractsController : BaseController<ItContract>
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
        }

        // GET /Organizations(1)/ItContracts
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/ItContracts")]
        public IHttpActionResult GetItContracts(int key)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }

        // GET /Organizations(1)/ItContracts(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItContracts({contractKey})")]
        public IHttpActionResult GetItContracts(int orgKey, int contractKey)
        {
            var result = Repository.AsQueryable().Where(m => m.Id == contractKey && (m.OrganizationId == orgKey));
            return Ok(result);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItContracts")]
        public IHttpActionResult GetItContractsByOrgUnit(int orgKey, int unitKey)
        {
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
                    .First(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);

                contracts.AddRange(orgUnit.ResponsibleForItContracts);

                var childIds = orgUnit.Children.Select(x => x.Id);
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return Ok(contracts);
        }
    }
}
