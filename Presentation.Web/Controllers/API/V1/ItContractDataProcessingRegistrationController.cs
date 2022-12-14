using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/it-contract/{id}/gdpr/{id}")]
    public class ItContractDataProcessingRegistrationController : BaseApiController
    {
        //public HttpResponseMessage 
    }
}