using Core.DomainModel.LocalOptions;
using Core.DomainModel.Reports;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalReportCategoryTypesController : LocalOptionBaseController<LocalReportCategoryType, Report, ReportCategoryType>
    {
        public LocalReportCategoryTypesController(IGenericRepository<LocalReportCategoryType> repository, IGenericRepository<ReportCategoryType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
