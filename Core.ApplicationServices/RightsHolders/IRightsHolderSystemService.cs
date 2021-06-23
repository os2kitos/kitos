using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.RightsHolders
{
    /// <summary>
    /// Application service which implements the use cases specific to rights holders KITOS access
    /// </summary>
    public interface IRightsHolderSystemService
    {
        IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess();
        Result<IQueryable<ItSystem>, OperationError> GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(List<IDomainQuery<ItSystem>> refinements, Guid? rightsHolderUuid = null);
        Result<ItSystem, OperationError> GetSystemAsRightsHolder(Guid systemUuid);
        Result<ItSystem, OperationError> CreateNewSystem(Guid rightsHolderUuid, RightsHolderSystemCreationParameters creationParameters);
        Result<ItSystem, OperationError> Update(Guid systemUuid, RightsHolderSystemUpdateParameters updateParameters);
        Result<ItSystem, OperationError> Deactivate(Guid systemUuid, string reason);
    }
}
