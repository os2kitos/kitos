using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Types.Shared;
using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class ItSystemResponseMapper : IItSystemResponseMapper
    {
        private readonly IExternalReferenceResponseMapper _referenceResponseMapper;

        public ItSystemResponseMapper(IExternalReferenceResponseMapper referenceResponseMapper)
        {
            _referenceResponseMapper = referenceResponseMapper;
        }

        public RightsHolderItSystemResponseDTO ToRightsHolderResponseDTO(ItSystem itSystem)
        {
            var dto = new RightsHolderItSystemResponseDTO
            {
                ExternalReferences = [],
                KLE = [],
                MainContractSuppliers = [], 
                Name = itSystem.Name,
                CreatedBy = itSystem.ObjectOwner?.MapIdentityNamePairDTO()!,
                RecommendedArchiveDuty = new RecommendedArchiveDutyResponseDTO(itSystem.ArchiveDutyComment, itSystem.ArchiveDuty?.ToChoice() ?? RecommendedArchiveDutyChoice.Undecided)
            };
            MapBaseInformation(itSystem, dto);
            return dto;
        }

        public ItSystemResponseDTO ToSystemResponseDTO(ItSystem itSystem)
        {
            var dto = new ItSystemResponseDTO
            {
                UsingOrganizations = itSystem
                    .Usages
                    .Select(systemUsage => systemUsage.Organization)
                    .Select(organization => organization.MapShallowOrganizationResponseDTO())
                    .ToList(),
                Name = itSystem.Name,
                LastModified = itSystem.LastChanged,
                CreatedBy = itSystem.ObjectOwner?.MapIdentityNamePairDTO()!,
                LastModifiedBy = itSystem.LastChangedByUser?.MapIdentityNamePairDTO()!,
                Scope = itSystem.AccessModifier.ToChoice(),
                OrganizationContext = itSystem.Organization?.MapShallowOrganizationResponseDTO()!,
                LegalName = itSystem.LegalName,
                LegalDataProcessorName = itSystem.LegalDataProcessorName,
                RecommendedArchiveDuty = new RecommendedArchiveDutyResponseDTO(itSystem.ArchiveDutyComment, itSystem.ArchiveDuty?.ToChoice() ?? RecommendedArchiveDutyChoice.Undecided),
                ExternalReferences = [],
                KLE = [],
                MainContractSuppliers = []
            };

            MapBaseInformation(itSystem, dto);

            return dto;
        }

        public ItSystemPermissionsResponseDTO MapPermissions(SystemPermissions permissions)
        {
            return new ItSystemPermissionsResponseDTO
            {
                Delete = permissions.BasePermissions.Delete,
                Modify = permissions.BasePermissions.Modify,
                Read = permissions.BasePermissions.Read,
                DeletionConflicts = permissions.DeletionConflicts.Select(MapConflict).ToList(),
                ModifyVisibility = permissions.ModifyVisibility
            };
        }

        private static Models.API.V2.Types.System.SystemDeletionConflict MapConflict(Core.ApplicationServices.Model.System.SystemDeletionConflict arg)
        {
            return arg.ToChoice();
        }

        private void MapBaseInformation<T>(ItSystem itSystem, T dto) where T : BaseItSystemResponseDTO
        {
            dto.Uuid = itSystem.Uuid;
            dto.ExternalUuid = itSystem.ExternalUuid;
            dto.RightsHolder = itSystem.BelongsTo?.Transform(organization => organization.MapShallowOrganizationResponseDTO());
            dto.BusinessType = itSystem.BusinessType?.Transform(businessType => businessType.MapIdentityNamePairDTO());
            dto.Description = itSystem.Description;
            dto.CreatedBy = itSystem.ObjectOwner?.MapIdentityNamePairDTO()!;
            dto.Created = itSystem.Created;
            dto.Deactivated = itSystem.Disabled;
            dto.FormerName = itSystem.PreviousName;
            dto.ParentSystem = itSystem.Parent?.Transform(parent => parent.MapIdentityNamePairDTO());
            dto.ExternalReferences = _referenceResponseMapper.MapExternalReferences(itSystem.ExternalReferences).ToList();
            dto.KLE = itSystem
                .TaskRefs
                .Select(taskRef => taskRef.MapIdentityNamePairDTO())
                .ToList();
            dto.MainContractSuppliers =
                itSystem.Usages.Select(x => x.MainContract?.ItContract.Supplier)
                    .Where(x => x != null)
                    .DistinctBy(x => x?.Uuid)
                    .Select(x => x!.MapShallowOrganizationResponseDTO())
                    .ToList();
        }
    }
}
