using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using System.Collections.Generic;

namespace Core.ApplicationServices.Rights
{
    public interface IUserRightsService
    {
        Result<IEnumerable<(User, Organization)>, OperationError> GetUsersAndOrganizationsWhereUserHasRightsholderAccess();
    }
}
