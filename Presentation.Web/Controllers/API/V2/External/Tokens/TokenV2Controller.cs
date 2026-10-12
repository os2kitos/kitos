using System.Linq;
using Presentation.Web.Models.API.V2.Request.Token;
using Presentation.Web.Infrastructure.Attributes;
using System.Net;
using Core.ApplicationServices.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.ApplicationServices.Model.Authentication;

namespace Presentation.Web.Controllers.API.V2.External.Tokens
{
    [Route("api/v2/token")]
    public class TokenV2Controller : ExternalBaseController
    {
        private readonly ITokenValidator _tokenValidator;
        public TokenV2Controller(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        [HttpPost]
        [Route("validate")]
        [AllowAnonymous]
        [IgnoreCSRFProtection]
        [ApiResponse(typeof(TokenIntrospectionResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult Introspect([FromBody] TokenIntrospectionRequest request)
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


