using System.Web.Http;
using Core.ApplicationServices.GDPR;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    /// <summary>
    /// Search API used for DataProcessingRegistrations
    /// </summary>
    [InternalApi]
    [ODataRoutePrefix("Organizations({organizationId})/DataProcessingRegistrationReadModels")]
    public class DataProcessingRegistrationReadModelsController : BaseOdataController
    {
        private readonly IDataProcessingRegistrationReadModelService _dataProcessingRegistrationReadModelService;

        public DataProcessingRegistrationReadModelsController(IDataProcessingRegistrationReadModelService dataProcessingRegistrationReadModelService)
        {
            _dataProcessingRegistrationReadModelService = dataProcessingRegistrationReadModelService;
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        [ODataRoute]
        public IHttpActionResult Get([FromODataUri]int organizationId)
        {
            return
                _dataProcessingRegistrationReadModelService
                    .GetByOrganizationId(organizationId)
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}
