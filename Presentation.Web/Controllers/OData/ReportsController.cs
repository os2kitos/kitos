using Core.DomainModel.Reports;
using Core.DomainServices;
using Core.ApplicationServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class ReportsController : BaseEntityController<Report>
    {
        public ReportsController(IGenericRepository<Report> repository, IAuthenticationService authService)
            : base(repository, authService)
        {}
    }
}
