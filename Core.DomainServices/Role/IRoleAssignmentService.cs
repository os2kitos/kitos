using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel;


namespace Core.DomainServices.Role
{
    public interface IRoleAssignmentService<TRight, out TRole, in TModel>
        where TRight : Entity, IRight<TModel, TRight, TRole>
        where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
        where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
    {
        IEnumerable<TRole> GetApplicableRoles(TModel registration); 
        Result<IQueryable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(TModel registration, int roleId, Maybe<string> nameEmailQuery);
        Result<TRight, OperationError> AssignRole(TModel model, int roleId, int userId);
        Result<TRight, OperationError> AssignRole(TModel model, Guid roleUuid, Guid userUuid);
        Result<TRight, OperationError> RemoveRole(TModel model, int roleId, int userId);
        Result<TRight, OperationError> RemoveRole(TModel model, Guid roleUuid, Guid userUuid);
        Maybe<OperationError> BatchUpdateRoles(TModel model, IEnumerable<(Guid roleUuid, Guid userUuid)> roleAssignments);
    }
}
