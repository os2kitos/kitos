
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.RightsHolders
{
    public interface IRightsHoldersService
    {
        IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess();
    }
}
