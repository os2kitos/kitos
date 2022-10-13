using Core.ApplicationServices.Rights;
using Core.Abstractions.Types;
using Core.DomainModel.Commands;

namespace Core.ApplicationServices.Users.Handlers
{
    public class RemoveUserFromOrganizationCommandHandler : ICommandHandler<RemoveUserFromOrganizationCommand, Maybe<OperationError>>
    {
        private readonly IUserRightsService _userRightsService;

        public RemoveUserFromOrganizationCommandHandler(IUserRightsService userRightsService)
        {
            _userRightsService = userRightsService;
        }

        public Maybe<OperationError> Execute(RemoveUserFromOrganizationCommand command)
        {
            return _userRightsService.RemoveAllRights(command.User.Id, command.OrganizationId);
        }
    }
}
