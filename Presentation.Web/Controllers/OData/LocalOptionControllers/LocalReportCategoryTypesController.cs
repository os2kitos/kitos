using Core.ApplicationServices;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalReportCategoryTypesController : LocalOptionBaseController<LocalReportCategoryType, Report, ReportCategoryType>
    {
        public LocalReportCategoryTypesController(IGenericRepository<LocalReportCategoryType> repository, IAuthenticationService authService, IGenericRepository<ReportCategoryType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
