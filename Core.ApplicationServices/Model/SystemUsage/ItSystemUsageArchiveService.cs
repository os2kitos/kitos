using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Archive;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Organizations;

namespace Core.ApplicationServices.Model.SystemUsage
{
    public class ItSystemUsageArchiveService(
        IItSystemUsageService systemUsageService,
        IOrganizationService organizationService,
        IAuthorizationContext authorizationContext,
        IOrganizationalUserContext organizationalUserContext,
        IGenericRepository<ItSystemUsageArchive> archiveRepository) : IItSystemUsageArchiveService
    {
        public Result<ItSystemUsageArchive, OperationError> Create(Guid systemUsageUuid, ArchiveItSystemUsageParameters parameters)
        {
            var systemUsageResult = systemUsageService.GetItSystemUsageByUuidAndAuthorizeRead(systemUsageUuid);
            if (systemUsageResult.Failed) return systemUsageResult.Error;
            var systemUsage = systemUsageResult.Value;

            if (!authorizationContext.AllowCreate<ItSystemUsageArchive>(systemUsage.OrganizationId))
                return new OperationError("User is not allowed to create it-system usage archives", OperationFailure.Forbidden);

            var snapshot = CreateSnapshot(systemUsage);
            var archiveReferences = CreateArchiveReferences(parameters);
            var archive = CreateArchive(systemUsage, parameters, snapshot, archiveReferences, organizationalUserContext.UserId);

            var createdArchive = archiveRepository.Insert(archive);
            archiveRepository.Save();
            return createdArchive;
        }

        public Result<ItSystemUsageArchive, OperationError> GetByUuid(Guid archiveUuid)
        {
            var archive = archiveRepository.AsQueryable()
                .FirstOrDefault(a => a.Uuid == archiveUuid);

            if (archive == null)
                return new OperationError($"Archive with UUID {archiveUuid} not found", OperationFailure.NotFound);

            if (!authorizationContext.AllowReads(archive))
                return new OperationError("User is not allowed to read this archive", OperationFailure.Forbidden);

            return archive;
        }

        public Result<ItSystemUsageArchive, OperationError> Delete(Guid archiveUuid)
        {
            return GetByUuid(archiveUuid)
                .Bind(archive => !authorizationContext.AllowDelete(archive)
                    ? new OperationError("User is not allowed to delete this archive", OperationFailure.Forbidden)
                    : Result<ItSystemUsageArchive, OperationError>.Success(archive))
                .Bind(archive =>
                {
                    archiveRepository.Delete(archive);
                    archiveRepository.Save();
                    return Result<ItSystemUsageArchive,OperationError>.Success(archive);
                });
        }
        
        public Result<ResourcePermissionsResult, OperationError> GetPermissions(Guid uuid)
        {
            return GetByUuid(uuid)
                .Transform(result => ResourcePermissionsResult.FromResolutionResult(result, authorizationContext));
        }

        public Result<ResourceCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid)
        {
            return organizationService
                .GetOrganization(organizationUuid)
                .Select(result => ResourceCollectionPermissionsResult.FromOrganizationId<ItSystemUsage>(result.Id, authorizationContext));
        }

        private static ItSystemUsageArchiveSnapshot CreateSnapshot(ItSystemUsage systemUsage)
        {
            var system = systemUsage.ItSystem;
            return new ItSystemUsageArchiveSnapshot
            {
                ItSystemUuid = system.Uuid,
                LegacyName = system.Name,
                TakenIntoUsageDate = systemUsage.Concluded,
                LocalId = systemUsage.LocalSystemId,
                LocalName = systemUsage.LocalCallName
            };
        }

        private static List<ArchiveReference> CreateArchiveReferences(ArchiveItSystemUsageParameters parameters)
        {
            return (parameters.ArchiveReferences ?? [])
                .Select(reference => new ArchiveReference
                {
                    Label = reference.Name,
                    Url = reference.Url
                })
                .ToList();
        }

        private static ItSystemUsageArchive CreateArchive(ItSystemUsage systemUsage, ArchiveItSystemUsageParameters parameters, ItSystemUsageArchiveSnapshot snapshot, List<ArchiveReference> archiveReferences, int archivedById)
        {
            return new ItSystemUsageArchive
            {
                SnapshotUuid = snapshot.Uuid,
                OrganizationId = systemUsage.OrganizationId,
                Organization = systemUsage.Organization,
                ArchivingDate = parameters.ArchivingDate,
                ReferenceName = parameters.ReferenceName,
                Note = parameters.Note,
                ArchivedById = archivedById,
                Snapshot = snapshot,
                ArchiveReferences = archiveReferences
            };
        }
    }
}
