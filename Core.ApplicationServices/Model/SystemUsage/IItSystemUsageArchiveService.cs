using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Archive;
using System;

namespace Core.ApplicationServices.Model.SystemUsage
{
    public interface IItSystemUsageArchiveService
    {
        Result<ItSystemUsageArchive, OperationError> Create(Guid systemUsageUuid, ArchiveItSystemUsageParameters parameters);
        Result<ItSystemUsageArchive, OperationError> GetByUuid(Guid archiveUuid);
        Result<ItSystemUsageArchive, OperationError> Delete(Guid archiveUuid);
        Result<ResourcePermissionsResult, OperationError> GetPermissions(Guid uuid);
        Result<ResourceCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid);
    }
}
