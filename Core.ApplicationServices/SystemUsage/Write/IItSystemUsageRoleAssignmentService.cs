using System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public interface IItSystemUsageRoleAssignmentService
    {
        Result<ItSystemRight, OperationError> AssignRole(ItSystemUsage systemUsage, int roleId, int userId);
        Result<ItSystemRight, OperationError> RemoveRole(ItSystemUsage systemUsage, int roleId, int userId);
        Result<ItSystemRight, OperationError> AssignRole(ItSystemUsage systemUsage, Guid roleUuid, Guid userUuid);
        Result<ItSystemRight, OperationError> RemoveRole(ItSystemUsage systemUsage, Guid roleUuid, Guid userUuid);
    }
}