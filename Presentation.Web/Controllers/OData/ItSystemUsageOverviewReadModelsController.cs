using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    [ODataRoutePrefix("Organizations({organizationId})/ItSystemUsageOverviewReadModels")]
    public class ItSystemUsageOverviewReadModelsController : BaseOdataController
    {
        private readonly IItsystemUsageOverviewReadModelsService _readModelsService;

        public ItSystemUsageOverviewReadModelsController(IItsystemUsageOverviewReadModelsService readModelsService)
        {
            _readModelsService = readModelsService;
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        [ODataRoute]
        public IHttpActionResult Get([FromODataUri] int organizationId)
        {
            return
                _readModelsService
                    .GetByOrganizationId(organizationId)
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}