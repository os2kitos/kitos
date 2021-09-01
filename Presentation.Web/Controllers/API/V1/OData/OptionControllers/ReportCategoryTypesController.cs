using Core.DomainModel.Reports;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class ReportCategoryTypesController : BaseEntityController<ReportCategoryType>
    {
        public ReportCategoryTypesController(IGenericRepository<ReportCategoryType> repository)
            : base(repository)
        {
        }
    }
}
