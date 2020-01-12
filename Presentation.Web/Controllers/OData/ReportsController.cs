using Core.DomainModel.Reports;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class ReportsController : BaseEntityController<Report>
    {
        public ReportsController(IGenericRepository<Report> repository)
            : base(repository)
        {}
    }
}
