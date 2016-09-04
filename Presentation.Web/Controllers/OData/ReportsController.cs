using System.Linq;
using System.Web.Http;
using Core.DomainModel.Reports;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ReportsController : BaseEntityController<Report>
    {
        private readonly IUserService _userService;

        public ReportsController(IGenericRepository<Report> repository, IUserService userService)
            : base(repository)
        {
            _userService = userService;
        }

        public override IHttpActionResult Get()
        {
            if (AuthenticationService.HasReadAccessOutsideContext(UserId))
                return base.Get();

            var orgId = _userService.GetCurrentOrganizationId(UserId);
            return Ok(Repository.AsQueryable().Where(x => x.OrganizationId == orgId));
        }
    }
}
