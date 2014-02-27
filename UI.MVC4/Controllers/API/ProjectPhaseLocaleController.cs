using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ProjectPhaseLocaleController : ApiController
    {

        protected readonly IProjectPhaseLocaleRepository Repository;

        protected ProjectPhaseLocaleController(IProjectPhaseLocaleRepository repository)
        {
            Repository = repository;
        }

        // GET api/ProjectPhaseLocale
        public ProjectPhaseLocale Get(int mId, int pId)
        {
            return Repository.GetById(mId, pId);
        }
    }
}