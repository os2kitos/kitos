using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ProjectPhaseLocaleController : GenericLocaleApiController<ProjPhaseLocale, ProjectPhase>
    {
        public ProjectPhaseLocaleController(IGenericRepository<ProjPhaseLocale> repository, IUserRepository userRepository) : base(repository, userRepository)
        {
        }
    }
}