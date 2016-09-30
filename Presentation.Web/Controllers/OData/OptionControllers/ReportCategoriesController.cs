using Core.ApplicationServices;
using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ReportCategoriesController : BaseEntityController<ReportCategoryType>
    {
        public ReportCategoriesController(IGenericRepository<ReportCategoryType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
