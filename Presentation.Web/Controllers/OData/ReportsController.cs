using System.Web.Http;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ReportsController : BaseEntityController<Report>
    {
        public ReportsController(IGenericRepository<Report> repository)
            : base(repository)
        {}

        public override IHttpActionResult Get()
        {
            if (AuthenticationService.HasReadAccessOutsideContext(UserId))
                return base.Get();

            var orgId = UserService.GetCurrentOrganizationId(UserId);
            return Ok(Repository.AsQueryable().Where(x => x.OrganizationId == orgId));
        }

        public override IHttpActionResult Post(Report entity)
        {
            entity.OrganizationId = CurentUser.DefaultOrganization.Id;

            return base.Post(entity);
        }

        // GET /Organizations(1)/Reports
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/Reports")]
        public IHttpActionResult GetItContracts(int key)
        {
            var loggedIntoOrgId = UserService.GetCurrentOrganizationId(CurentUser.Id);
            if (loggedIntoOrgId != key && !AuthenticationService.HasReadAccessOutsideContext(CurentUser))
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }
    }
}
