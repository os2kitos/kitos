using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class DataworkerController : GenericApiController<ItSystemDataWorkerRelation, ItSystemDataWorkerRelationDTO>
    {
        public DataworkerController(IGenericRepository<ItSystemDataWorkerRelation> repository)
            : base(repository)
        {
        }
    }
}