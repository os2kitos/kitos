using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Authentication.Commands;
using Core.DomainModel;
using Core.DomainModel.Commands;
using Core.DomainServices;
using Infrastructure.Services.Cryptography;
using Serilog;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ValidateUserCredentialsCommandHandler : ICommandHandler<ValidateUserCredentialsCommand, Result<User, OperationError>>
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
            var user = _userRepository.GetByEmail(command.Email);
            if (user == null)
            {
                _logger.Information("AUTH FAILED: Login attempt for unknown user email {hashEmail}", _cryptoService.Encrypt(command.Email ?? ""));
                return new OperationError(OperationFailure.BadInput);
            }

            if (user.Deleted)
            {
                _logger.Warning("AUTH FAILED: Attempt to login as deleted user with id:{id}", user.Id);
                return new OperationError(OperationFailure.BadInput);
            }

            // Validate password using the same logic as CustomMembershipProvider
            var expectedHash = _cryptoService.Encrypt((command.Password ?? "") + user.Salt);
            if (user.Password != expectedHash)
            {
                _logger.Information("AUTH FAILED: Attempt to login with bad credentials for {hashEmail}", _cryptoService.Encrypt(command.Email ?? ""));
                return new OperationError(OperationFailure.BadInput);
            }

            if (!user.GetAuthenticationSchemes().Contains(command.AuthenticationScheme))
            {
                _logger.Information("AUTH FAILED: User with id {userId} attempted login with disallowed scheme {scheme}", user.Id, command.AuthenticationScheme);
                return new OperationError(OperationFailure.BadInput);
            }

            return user;
        }
    }
}
