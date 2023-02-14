using System.Web.Http;
using Presentation.Web.Controllers.API.V2.Common;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V2.External
{
    [PublicApi]
    [Authorize]
    public class ExternalBaseController: ApiV2Controller
    {
    }
}