using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.Reports;
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
