using System.Web.Http;
using System.Web.OData.Routing;
using Core.DomainModel.Reports;
using Core.DomainServices;
using Core.ApplicationServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ReportsController : BaseEntityController<Report>
    {
        public ReportsController(IGenericRepository<Report> repository, IAuthenticationService authService)
            : base(repository, authService)
        {}

        // GET /Organizations(1)/Reports
        [ODataRoute("Organizations({key})/Reports")]
        public IHttpActionResult GetReportsByOrganization(int key)
        {
            return GetByOrganizationKey(key);
        }
    }
}
