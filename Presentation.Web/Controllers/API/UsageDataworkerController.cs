using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class UsageDataworkerController : GenericApiController<ItSystemUsageDataWorkerRelation, ItSystemUsageDataWorkerRelationDTO>
    {
        public UsageDataworkerController(IGenericRepository<ItSystemUsageDataWorkerRelation> repository)
            : base(repository)
        {
        }
    }
}