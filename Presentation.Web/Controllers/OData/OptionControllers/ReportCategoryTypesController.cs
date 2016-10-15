using Core.ApplicationServices;
using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ReportCategoryTypesController : BaseEntityController<ReportCategoryType>
    {
        public ReportCategoryTypesController(IGenericRepository<ReportCategoryType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
