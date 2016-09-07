using System.Web.Http;
using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ReportsController : BaseEntityController<Report>
    {
        public ReportsController(IGenericRepository<Report> repository)
            : base(repository)
        {
        }

        public override IHttpActionResult Post(Report entity)
        {
            entity.OrganizationId = CurentUser.DefaultOrganization.Id;

            return base.Post(entity);
        }
    }
}
