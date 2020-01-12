using Core.DomainModel.Reports;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class ReportCategoryTypesController : BaseEntityController<ReportCategoryType>
    {
        public ReportCategoryTypesController(IGenericRepository<ReportCategoryType> repository)
            : base(repository)
        {
        }
    }
}
