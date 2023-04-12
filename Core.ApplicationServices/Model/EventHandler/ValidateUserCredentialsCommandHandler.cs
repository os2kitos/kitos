using System.Linq;
using System.Web.Security;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Authentication.Commands;
using Core.DomainModel;
using Core.DomainModel.Commands;
using Core.DomainServices;
using Infrastructure.Services.Cryptography;
using Serilog;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ValidateUserCredentialsCommandHandler : ICommandHandler<ValidateUserCredentialsCommand,Result<User,OperationError>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger _logger;

        public ValidateUserCredentialsCommandHandler(IUserRepository userRepository, ICryptoService cryptoService, ILogger logger)
        {
            _userRepository = userRepository;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        public Result<User, OperationError> Execute(ValidateUserCredentialsCommand command)
        {
            if (!Membership.ValidateUser(command.Email, command.Password))
            {
                _logger.Information("AUTH FAILED: Attempt to login with bad credentials for {hashEmail}", _cryptoService.Encrypt(command.Email ?? ""));
                {
                    return new OperationError(OperationFailure.BadInput);
                }
            }

            var user = _userRepository.GetByEmail(command.Email);
            if (user == null)
            {
                _logger.Error("AUTH FAILED: User found during membership validation but could not be found by email: {hashEmail}", _cryptoService.Encrypt(command.Email));
                {
                    return new OperationError(OperationFailure.BadInput);
                }
            }

            if (user.GetAuthenticationSchemes().Contains(command.AuthenticationScheme))
            {
                return user;
            }

            _logger.Information("'AUTH FAILED: Non-global admin' User with id {userId} and no organization rights or wrong scheme {scheme} denied access", user.Id, command.AuthenticationScheme);
            {
                return new OperationError(OperationFailure.BadInput);
            }
        }
    }
}
