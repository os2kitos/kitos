
using System;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.RightsHolders
{
    public interface IRightsHoldersService
    {
        IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess();

        Result<IQueryable<ItInterface>, OperationError> GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(Guid? rightsHolderUuid = null);
        Result<ItInterface, OperationError> GetInterfaceAsRightsHolder(Guid interfaceUuid);
    }
}
