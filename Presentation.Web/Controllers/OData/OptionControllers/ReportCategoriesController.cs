using Core.ApplicationServices;
using Core.DomainModel.Reports;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ReportCategoriesController : BaseEntityController<ReportCategoryType>
    {
        public ReportCategoriesController(IGenericRepository<ReportCategoryType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
