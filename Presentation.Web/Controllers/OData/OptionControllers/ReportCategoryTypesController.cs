using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReportCategoryTypesController : BaseEntityController<ReportCategoryType>
    {
        public ReportCategoryTypesController(IGenericRepository<ReportCategoryType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
