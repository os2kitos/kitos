using System.Web.Http;
using Presentation.Web.Controllers.API.V2.Common;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V2.Internal
{
    [InternalApi]
    [Authorize]
    public class InternalApiV2Controller : ApiV2Controller
    {
    }
}