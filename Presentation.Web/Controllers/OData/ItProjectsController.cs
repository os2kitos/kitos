using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItProjectsController : BaseController<ItProject>
    {
        public ItProjectsController(IGenericRepository<ItProject> repository)
            : base(repository)
        {
        }

        [EnableQuery]
        [ODataRoute("ItProjects")]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItProjects")]
        public IHttpActionResult GetItSystems(int key)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
            return Ok(result);
        }

        // GET /Organizations(1)/ItSystems(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItProjects({projKey})")]
        public IHttpActionResult GetItSystems(int orgKey, int projKey)
        {
            var result = Repository.AsQueryable().Where(m => m.Id == projKey && (m.OrganizationId == orgKey || m.AccessModifier == AccessModifier.Public));
            return Ok(result);
        }
    }
}
