using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [ODataRoutePrefix("LocalArchiveLocations")]
    public class LocalArchiveLocationsController : LocalOptionBaseController<LocalArchiveLocation, ItSystemUsage, ArchiveLocation>
    {
        public LocalArchiveLocationsController(IGenericRepository<LocalArchiveLocation> repository, IGenericRepository<ArchiveLocation> optionsRepository)
            : base(repository, optionsRepository)
        {
        }

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult GetByOrganizationId(int organizationId) => base.GetByOrganizationId(organizationId);

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult Get(int organizationId, int key) => base.Get(organizationId, key);

        [ODataRoute]
        public override IHttpActionResult Post(int organizationId, LocalArchiveLocation entity) => base.Post(organizationId, entity);

        [ODataRoute]
        public override IHttpActionResult Patch(int organizationId, int key, Delta<LocalArchiveLocation> delta) => base.Patch(organizationId, key, delta);

        [ODataRoute]
        public override IHttpActionResult Delete(int organizationId, int key) => base.Delete(organizationId, key);
    }
}