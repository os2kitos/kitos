
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.RightsHolders
{
    public interface IRightsHoldersService
    {
        Result<IQueryable<Organization>, OperationError> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess();
    }
}
