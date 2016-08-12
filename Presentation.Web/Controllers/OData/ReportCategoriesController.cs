using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ReportCategoriesController : BaseController<ReportCategoryType>
    {
        public ReportCategoriesController(IGenericRepository<ReportCategoryType> repository)
            : base(repository)
        {



        }
    }
}