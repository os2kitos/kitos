using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ReportsController : BaseController<Report>
    {
        public ReportsController(IGenericRepository<Report> repository)
            : base(repository)
        {
        }
    }
}
