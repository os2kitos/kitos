using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ArchivePeriodsController : BaseEntityController<ArchivePeriod>
    {
        // GET: ArchivePeriode
        public ArchivePeriodsController(IGenericRepository<ArchivePeriod> repository, IAuthenticationService authService)
            : base(repository)
        {
        }
    }
}