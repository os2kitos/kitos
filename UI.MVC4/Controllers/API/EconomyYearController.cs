using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
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
