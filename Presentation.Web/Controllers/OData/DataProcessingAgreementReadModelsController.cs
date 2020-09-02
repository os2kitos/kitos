using System.Web.Http;
using Core.ApplicationServices.GDPR;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    /// <summary>
    /// Search API used for DataProcessingAgreements
    /// </summary>
    [InternalApi]
    [ODataRoutePrefix("Organizations({organizationId})/DataProcessingAgreementReadModels")]
    public class DataProcessingAgreementReadModelsController : BaseOdataController
    {
        private readonly IDataProcessingAgreementReadService _dataProcessingAgreementReadService;

        public DataProcessingAgreementReadModelsController(IDataProcessingAgreementReadService dataProcessingAgreementReadService)
        {
            _dataProcessingAgreementReadService = dataProcessingAgreementReadService;
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        [ODataRoute]
        public IHttpActionResult Get([FromODataUri]int organizationId)
        {
            return
                _dataProcessingAgreementReadService
                    .GetByOrganizationId(organizationId)
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}
