using Presentation.Web.Infrastructure;
using System.Web.Http;
using Presentation.Web.Models.API.V2.Request.Token;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Controllers.API.V2.External.Tokens
{
    [RoutePrefix("api/v2/token")]
    public class TokenV2Controller : ExternalBaseController
    {
        [HttpPost]
        [Route("validate")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(TokenIntrospectionResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [AllowAnonymous]
        [IgnoreCSRFProtection]
        public IHttpActionResult Introspect([FromBody] TokenIntrospectionRequest request)
        {
            return new TokenValidator().VerifyToken(request.Token)
                .Match(Ok, FromOperationError);
        }
    }
}