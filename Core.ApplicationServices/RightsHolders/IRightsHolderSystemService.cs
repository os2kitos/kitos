using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.RightsHolders
{
    public interface IRightsHolderSystemService
    {
        IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess();
        Result<IQueryable<ItSystem>, OperationError> GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(IEnumerable<IDomainQuery<ItSystem>> refinements, Guid? rightsHolderUuid = null);
        Result<ItSystem, OperationError> GetSystemAsRightsHolder(Guid systemUuid);
        Result<ItSystem, OperationError> CreateNewSystemAsRightsHolder(Guid rightsHolderUuid, RightsHolderSystemCreationParameters creationParameters);
        Result<ItSystem, OperationError> UpdateAsRightsHolder(Guid systemUuid, RightsHolderSystemUpdateParameters updateParameters);
        Result<ItSystem, OperationError> DeactivateAsRightsHolder(Guid systemUuid, string reason);
    }
}
