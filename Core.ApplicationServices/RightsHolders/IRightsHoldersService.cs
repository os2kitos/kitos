using System;
using System.Linq;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.RightsHolders
{
    /// <summary>
    /// Application service which implements the use cases specific to rights holders KITOS access
    /// </summary>
    public interface IRightsHoldersService //TODO: Rename to IRightsHoldersystemService
    {
        IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess(); //TODO: Move to a rightsholderOrganizatoinServicer
        Result<IQueryable<ItSystem>, OperationError> GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(Guid? rightsHolderUuid = null);
        Result<ItSystem, OperationError> GetSystemAsRightsHolder(Guid systemUuid);
        Result<ItSystem, OperationError> CreateNewSystem(Guid rightsHolderUuid, RightsHolderSystemCreationParameters creationParameters);
        Result<ItSystem, OperationError> Update(Guid systemUuid, RightsHolderSystemUpdateParameters updateParameters);
        Result<ItSystem, OperationError> Deactivate(Guid systemUuid, string reason);
    }
}
