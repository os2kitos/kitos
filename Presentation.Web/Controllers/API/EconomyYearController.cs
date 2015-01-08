using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class EconomyYearController : GenericApiController<EconomyYear, EconomyYearDTO>
    {
        public EconomyYearController(IGenericRepository<EconomyYear> repository) : base(repository)
        {
        }

        protected override EconomyYear PatchQuery(EconomyYear item)
        {
            CheckHasWriteAccess();

            return base.PatchQuery(item);
        }

        private void CheckHasWriteAccess()
        {
            //TODO
        }
    }
}
