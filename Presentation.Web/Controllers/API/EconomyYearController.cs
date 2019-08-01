using Core.DomainModel.ItProject;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class EconomyYearController : GenericContextAwareApiController<EconomyYear, EconomyYearDTO>
    {
        public EconomyYearController(IGenericRepository<EconomyYear> repository) : base(repository)
        {
        }

        protected override EconomyYear PatchQuery(EconomyYear item, JObject obj)
        {
            CheckHasWriteAccess();

            return base.PatchQuery(item, obj);
        }

        private void CheckHasWriteAccess()
        {
            //TODO
        }
    }
}
