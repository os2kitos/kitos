using System.Web.Http;
using Core.ApplicationServices.GDPR;
using Microsoft.AspNet.OData;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    /// <summary>
    /// Search API used for DataProcessingAgreements
    /// </summary>
    [PublicApi]
    public class DataProcessingAgreementsController : BaseOdataController
    {
        private readonly IDataProcessingAgreementReadService _dataProcessingAgreementReadService;

        public DataProcessingAgreementsController(IDataProcessingAgreementReadService dataProcessingAgreementReadService)
        {
            _dataProcessingAgreementReadService = dataProcessingAgreementReadService;
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetByOrganizationId(int organizationId)
        {
            return
                _dataProcessingAgreementReadService
                    .GetByOrganizationId(organizationId)
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}
