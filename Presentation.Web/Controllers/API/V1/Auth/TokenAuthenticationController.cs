using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Authentication.Commands;
using Core.DomainModel;
using Core.DomainModel.Commands;
using Presentation.Web.Infrastructure;
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

        public TokenAuthenticationController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
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
            if (loginDto == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest();
            }
            try
            {
                var result = _commandBus.Execute<ValidateUserCredentialsCommand,Result<User,OperationError>>(new ValidateUserCredentialsCommand(loginDto.Email,loginDto.Password,AuthenticationScheme.Token));

                if (result.Failed)
                {
                    return Unauthorized();
                }

                var user = result.Value;

                var token = new TokenValidator().CreateToken(user);

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
