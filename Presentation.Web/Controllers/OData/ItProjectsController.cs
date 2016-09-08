using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using System.Web.Http.Results;
using System.Net;
using Core.DomainModel.Organization;

namespace Presentation.Web.Controllers.OData
{
    [Authorize]
    public class ItProjectsController : BaseEntityController<ItProject>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public ItProjectsController(IGenericRepository<ItProject> repository, IGenericRepository<OrganizationUnit> orgUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        [EnableQuery]
        [ODataRoute("ItProjects")]
        public override IHttpActionResult Get()
        {
            return base.Get();

            //if (AuthenticationService.HasReadAccessOutsideContext(UserId))
            //    return base.Get();

            //var orgId = CurrentOrganizationId;
            //return Ok(Repository.AsQueryable().Where(x => x.OrganizationId == orgId));
        }

        // GET /Organizations(1)/ItProjects
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItProjects")]
        public IHttpActionResult GetItProjects(int key)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (!AuthenticationService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                    return new StatusCodeResult(HttpStatusCode.Forbidden, this);

                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
                return Ok(result);
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
                return Ok(result);
            }
        }

        // GET /Organizations(1)/ItProjects(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItProjects({projKey})")]
        public IHttpActionResult GetItProjects(int orgKey, int projKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == projKey);
            if (entity == null)
                return NotFound();

            if (AuthenticationService.HasReadAccess(UserId, entity))
                return Ok(entity);

            return new StatusCodeResult(HttpStatusCode.Forbidden, this);
        }

        // TODO for now only read actions are allowed, in future write will be enabled - but keep security in mind!
        // GET /Organizations(1)/OrganizationUnits(1)/ItProjects
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItProjects")]
        public IHttpActionResult GetItProjectsByOrgUnit(int orgKey, int unitKey)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (loggedIntoOrgId != orgKey && !AuthenticationService.HasReadAccessOutsideContext(UserId))
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

            var projects = new List<ItProject>();

            // using iteration instead of recursion else we're running into
            // an "multiple DataReaders open" issue and MySQL doesn't support MARS

            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                var orgUnit = _orgUnitRepository.AsQueryable()
                    .Include(x => x.Children)
                    .Include(x => x.UsingItProjects.Select(y => y.ResponsibleItProject))
                    .First(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);

                var responsibles = orgUnit.UsingItProjects.Select(x => x.ResponsibleItProject).Where(x => x != null);
                projects.AddRange(responsibles);

                var childIds = orgUnit.Children.Select(x => x.Id);
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return Ok(projects);
        }
    }
}
