using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace Presentation.Web.Controllers.API.V1.OData.LocalOptionControllers
{
    [InternalApi]
    [ODataRoutePrefix("LocalArchiveTestLocations")]
    public class LocalArchiveTestLocationsController : LocalOptionBaseController<LocalArchiveTestLocation, ItSystemUsage, ArchiveTestLocation>
    {
        public LocalArchiveTestLocationsController(IGenericRepository<LocalArchiveTestLocation> repository, IGenericRepository<ArchiveTestLocation> optionsRepository)
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
        public override IHttpActionResult Post(int organizationId, LocalArchiveTestLocation entity) => base.Post(organizationId, entity);

        [ODataRoute]
        public override IHttpActionResult Patch(int organizationId, int key, Delta<LocalArchiveTestLocation> delta) => base.Patch(organizationId, key, delta);

        [ODataRoute]
        public override IHttpActionResult Delete(int organizationId, int key) => base.Delete(organizationId, key);
    }
}