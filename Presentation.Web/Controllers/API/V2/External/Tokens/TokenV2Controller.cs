using System.Linq;
using System.Web.Http;
using Presentation.Web.Models.API.V2.Request.Token;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Model.Authentication;
using Presentation.Web.Properties;

namespace Presentation.Web.Controllers.API.V2.External.Tokens
{
    [RoutePrefix("api/v2/token")]
    public class TokenV2Controller : ExternalBaseController
    {
        private readonly ITokenValidator _tokenValidator;
        public TokenV2Controller(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        [HttpPost]
        [Route("validate")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(TokenIntrospectionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [AllowAnonymous]
        [IgnoreCSRFProtection]
        public IHttpActionResult Introspect([FromBody] TokenIntrospectionRequest request)
        {
            return _tokenValidator.VerifyToken(request.Token)
                .Select(MapTokenIntrospectionRequestToDTO)
                .Match(Ok, FromOperationError);
        }

        private static TokenIntrospectionResponseDTO MapTokenIntrospectionRequestToDTO(TokenIntrospectionResponse request)
        {
            return new TokenIntrospectionResponseDTO
            {
                Active = request.Active,
                Claims = request.Claims.Select(MapClaimResponseToDTO).ToList(),
                Expiration = request.Expiration
            };
        }

        private static ClaimResponseDTO MapClaimResponseToDTO(ClaimResponse claimResponse)
        {
            return new ClaimResponseDTO(claimResponse);
        }
    }
}