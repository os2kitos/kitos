using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.LocalOptions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace Presentation.Web.Controllers.API.V1.OData.LocalOptionControllers
{
    [InternalApi]
    [ODataRoutePrefix("LocalItSystemCategories")]
    public class LocalItSystemCategoriesController : LocalOptionBaseController<LocalItSystemCategories, ItSystemUsage, ItSystemCategories>
    {
        public LocalItSystemCategoriesController(IGenericRepository<LocalItSystemCategories> repository, IGenericRepository<ItSystemCategories> optionsRepository)
            : base(repository, optionsRepository)
        {
        }

        [EnableQuery]
        [ODataRoute]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult GetByOrganizationId(int organizationId) => base.GetByOrganizationId(organizationId);

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult Get(int organizationId, int key) => base.Get(organizationId, key);

        [ODataRoute]
        public override IHttpActionResult Post(int organizationId, LocalItSystemCategories entity) => base.Post(organizationId, entity);

        [ODataRoute]
        public override IHttpActionResult Patch(int organizationId, int key, Delta<LocalItSystemCategories> delta) => base.Patch(organizationId, key, delta);

        [ODataRoute]
        public override IHttpActionResult Delete(int organizationId, int key) => base.Delete(organizationId, key);
    }
}