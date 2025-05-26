using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Model.Authentication.Commands;
using Core.DomainModel;
using Core.DomainModel.Commands;
using Core.DomainModel.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;
using AuthenticationScheme = Core.DomainModel.Users.AuthenticationScheme;

namespace Presentation.Web.Controllers.API.V1.Auth
{
    [PublicApi]
    public class TokenAuthenticationController : ExtendedApiController
    {

        private readonly ICommandBus _commandBus;
        private readonly ITokenValidator _tokenValidator;

        public TokenAuthenticationController(ICommandBus commandBus, ITokenValidator tokenValidator)
        {
            _commandBus = commandBus;
            _tokenValidator = tokenValidator;
        }

        /// <summary>
        /// Issue a KITOS JWT
        /// Notes:
        /// - Credentials must belong to a user which has a membership in one or more organizations
        /// - The user must be of type 'API User'. For more info, check the KITOS API 'Getting started' documentation.
        /// - KITOS JWT are valid for 24 hours (see 'expires' field).
        /// - Read/Write permissions are provided by the Local Administrators in the individual municipalities
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/authorize/GetToken")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<GetTokenResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [IgnoreCSRFProtection]
        [AllowRightsHoldersAccess]
        public HttpResponseMessage GetToken(UserCredentialsDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var command = new ValidateUserCredentialsCommand(loginDto.Email, loginDto.Password, AuthenticationScheme.Token);
                var validationResult = _commandBus.ExecuteWithResult<ValidateUserCredentialsCommand, User>(command);

                if (validationResult.Failed)
                {
                    return Unauthorized();
                }

                var user = validationResult.Value;

                var token = _tokenValidator.CreateToken(user);

                var response = new GetTokenResponseDTO
                {
                    Token = token.Value,
                    Email = loginDto.Email,
                    LoginSuccessful = true,
                    Expires = token.Expiration
                };

                Logger.Info($"Created token for user with Id {user.Id}");

                return Ok(response);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create token");
                return LogError(e);
            }
        }
    }
}
