using System.Linq;
using System.Net.Http;
using System.Web.Security;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices;
using Infrastructure.Services.Cryptography;
using Presentation.Web.Models.API.V1;
using AuthenticationScheme = Core.DomainModel.Users.AuthenticationScheme;

namespace Presentation.Web.Controllers.API.V1.Auth
{
    public abstract class BaseAuthenticationController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ICryptoService _cryptoService;

        protected BaseAuthenticationController(
            IUserRepository userRepository,
            ICryptoService cryptoService)
        {
            _userRepository = userRepository;
            _cryptoService = cryptoService;
        }

        protected Result<User, HttpResponseMessage> AuthenticateUser(LoginDTO loginDto, AuthenticationScheme authenticationScheme)
        {
            if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
            {
                Logger.Info("AUTH FAILED: Attempt to login with bad credentials for {hashEmail}", _cryptoService.Encrypt(loginDto.Email ?? ""));
                {
                    return Unauthorized();
                }
            }

            var user = _userRepository.GetByEmail(loginDto.Email);
            if (user == null)
            {
                Logger.Error("AUTH FAILED: User found during membership validation but could not be found by email: {hashEmail}", _cryptoService.Encrypt(loginDto.Email));
                {
                    return Unauthorized();
                }
            }

            if (user.GetAuthenticationSchemes().Contains(authenticationScheme))
            {
                return user;
            }

            Logger.Info("'AUTH FAILED: Non-global admin' User with id {userId} and no organization rights or wrong scheme {scheme} denied access", user.Id, authenticationScheme);
            {
                return Unauthorized();
            }
        }
    }
}
