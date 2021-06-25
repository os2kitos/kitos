using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices.Rights
{
    public class UserRightsService : IUserRightsService
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationalUserContext _userContext;

        public UserRightsService(IUserService userService, IOrganizationService organizationService, IOrganizationalUserContext userContext)
        {
            _userService = userService;
            _organizationService = organizationService;
            _userContext = userContext;
        }


        public Result<IEnumerable<(User, Organization)>, OperationError> GetUsersAndOrganizationsWhereUserHasRightsholderAccess()
        {
            if (!_userContext.IsGlobalAdmin())
            {
                return new OperationError("User is not global admin", OperationFailure.Forbidden);
            }

            var result = new List<(User, Organization)>();

            var usersWithRightsholderAccess = _userService.GetUsersWithRightsHolderAccess();
            if (usersWithRightsholderAccess.Failed)
            {
                return usersWithRightsholderAccess.Error;
            }

            foreach (User user in usersWithRightsholderAccess.Value.ToList())
            {
                var organizationIds = user.GetOrganizationsWhereRightsholderAccess();

                var organizations = _organizationService.GetOrganizations(organizationIds)
                    .Select(x => x.ToList()
                        .Select(org => (user, org)));

                if (organizations.Failed)
                {
                    return organizations.Error;
                }

                result.AddRange(organizations.Value);
            }

            return result;
        }
    }
}
