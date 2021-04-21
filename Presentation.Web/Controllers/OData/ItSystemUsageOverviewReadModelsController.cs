using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
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
        public IHttpActionResult Get([FromODataUri] int organizationId)
        {
            return
                _readModelsService
                    .GetByOrganizationId(organizationId)
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }

        [ODataRoute("Organizations({organizationId})/OrganizationUnits({organizationUnitId})/ItSystemUsageOverviewReadModels")]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetByOrgUnit(int organizationId, int organizationUnitId)
        {
            return
                _readModelsService
                    .GetByOrganizationAndResponsibleOrganizationUnitId(organizationId, organizationUnitId)
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}