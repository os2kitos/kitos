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
        Result<IQueryable<ItSystem>, OperationError> GetSystemsWhereAuthenticatedUserHasRightsHolderAccess();
        Result<ItSystem, OperationError> GetSystemAsRightsHolder(Guid systemUuid);
        Result<IQueryable<ItInterface>, OperationError> GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(Guid? rightsHolderUuid = null);
        Result<ItInterface, OperationError> GetInterfaceAsRightsHolder(Guid interfaceUuid);
    }
}
