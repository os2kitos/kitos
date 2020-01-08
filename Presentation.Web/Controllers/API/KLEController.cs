using System.Net.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class KLEController : BaseApiController
    {
        private readonly IKLEApplicationService _kleApplicationService;

        public KLEController(IAuthorizationContext authorizationContext, IKLEApplicationService kleApplicationService) : base(authorizationContext)
        {
            _kleApplicationService = kleApplicationService;
        }

        public HttpResponseMessage GetKLEStatus()
        {
            var result = _kleApplicationService.GetKLEStatus();

            switch (result.Status)
            {
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.Ok:
                    return Ok(
                        new KLEStatusDTO
                        {
                            UpToDate = result.Value.UpToDate,
                            Version = result.Value.Published.ToLongDateString()
                        });
                default:
                    return Error($"Something went wrong getting KLE status");
            }
        }
    }
}