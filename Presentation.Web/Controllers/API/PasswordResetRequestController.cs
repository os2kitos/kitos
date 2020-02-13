using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Infrastructure.Services.Cryptography;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [AllowAnonymous]
    [PublicApi]
    public class PasswordResetRequestController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ICryptoService _cryptoService;

        public PasswordResetRequestController(IUserService userService, IUserRepository userRepository, ICryptoService cryptoService)
        {
            _userService = userService;
            _userRepository = userRepository;
            _cryptoService = cryptoService;
        }

        // POST api/PasswordResetRequest
        public HttpResponseMessage Post([FromBody] UserDTO input)
        {
            try
            {
                var user = _userRepository.GetByEmail(input.Email);
                if (user == null)
                {
                    //Intentional OK... do not be a membership db oracle
                    Logger.Info("User attempted to issue reset request for email {hashedEmail} which was not found in KITOS.", _cryptoService.Encrypt(input.Email ?? string.Empty));
                    return Ok();
                }
                if (!user.CanAuthenticate())
                {
                    Logger.Warn("User with id {userId} cannot authenticate and will not be issued a reset password email", user.Id);
                    return Ok();
                }

                _userService.IssuePasswordReset(user, null, null);

                return Ok();
            }
            catch (Exception e)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        // GET api/PasswordResetRequest
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<PasswordResetRequestDTO>))]
        public HttpResponseMessage Get(string requestId)
        {
            try
            {
                var request = _userService.GetPasswordReset(requestId);
                if (request == null) return NotFound();
                var dto = AutoMapper.Mapper.Map<PasswordResetRequest, PasswordResetRequestDTO>(request);

                var msg = CreateResponse(HttpStatusCode.OK, dto);
                return msg;
            }
            catch (Exception e)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
