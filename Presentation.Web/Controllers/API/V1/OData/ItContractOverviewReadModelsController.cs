using System.Web.Http;
using Core.ApplicationServices.Contract.ReadModels;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItContractOverviewReadModelsController : BaseOdataController
    {
        private readonly IItContractOverviewReadModelsService _readModelsService;

        public ItContractOverviewReadModelsController(IItContractOverviewReadModelsService readModelsService)
        {
            _readModelsService = readModelsService;
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        [ODataRoute("Organizations({organizationId})/ItContractOverviewReadModels")]
        public IHttpActionResult Get([FromODataUri] int organizationId, int? responsibleOrganizationUnitId = null)
        {
            var query = responsibleOrganizationUnitId == null
                ? _readModelsService.GetByOrganizationId(organizationId)
                : _readModelsService.GetByOrganizationIdAndIdOrgOrganizationUnitSubTree(organizationId,
                    responsibleOrganizationUnitId.Value);

            return query.Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}