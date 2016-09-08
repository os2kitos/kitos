using System.Web.Http;
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
        
        // GET /Organizations(1)/Reports
        [ODataRoute("Organizations({key})/Reports")]
        public IHttpActionResult GetItContracts(int key)
        {
            return GetByOrganizationKey(key);
        }
    }
}
