using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItSystemUsageOverviewReadModelsController : BaseOdataController
    {
        private readonly IItsystemUsageOverviewReadModelsService _readModelsService;

        public ItSystemUsageOverviewReadModelsController(IItsystemUsageOverviewReadModelsService readModelsService)
        {
            _readModelsService = readModelsService;
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        [ODataRoute("Organizations({organizationId})/ItSystemUsageOverviewReadModels")]
        public IHttpActionResult Get([FromODataUri] int organizationId, int? responsibleOrganizationUnitId = null)
        {
            var byOrganizationId = responsibleOrganizationUnitId == null
                ? _readModelsService.GetByOrganizationId(organizationId)
                : _readModelsService.GetByOrganizationAndResponsibleOrganizationUnitId(organizationId,
                    responsibleOrganizationUnitId.Value);
            return
                byOrganizationId
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}