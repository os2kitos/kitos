using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Types.Shared;

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
            var dto = new RightsHolderItSystemResponseDTO();
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
                LastModified = itSystem.LastChanged,
                LastModifiedBy = itSystem.LastChangedByUser?.MapIdentityNamePairDTO(),
                Scope = itSystem.AccessModifier.ToChoice(),
                OrganizationContext = itSystem.Organization?.MapShallowOrganizationResponseDTO(),
                LegalName = itSystem.LegalName,
                LegalDataProcessorName = itSystem.LegalDataProcessorName
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

        private static Presentation.Web.Models.API.V2.Types.System.SystemDeletionConflict MapConflict(SystemDeletionConflict arg)
        {
            return arg.ToChoice();
        }

        private void MapBaseInformation<T>(ItSystem arg, T dto) where T : BaseItSystemResponseDTO
        {
            dto.Uuid = arg.Uuid;
            dto.ExternalUuid = arg.ExternalUuid;
            dto.Name = arg.Name;
            dto.RightsHolder = arg.BelongsTo?.Transform(organization => organization.MapShallowOrganizationResponseDTO());
            dto.BusinessType = arg.BusinessType?.Transform(businessType => businessType.MapIdentityNamePairDTO());
            dto.Description = arg.Description;
            dto.CreatedBy = arg.ObjectOwner?.MapIdentityNamePairDTO();
            dto.Created = arg.Created;
            dto.Deactivated = arg.Disabled;
            dto.FormerName = arg.PreviousName;
            dto.ParentSystem = arg.Parent?.Transform(parent => parent.MapIdentityNamePairDTO());
            dto.ExternalReferences = _referenceResponseMapper.MapExternalReferences(arg.ExternalReferences).ToList();
            dto.RecommendedArchiveDuty = new RecommendedArchiveDutyResponseDTO(arg.ArchiveDutyComment, arg.ArchiveDuty?.ToChoice() ?? RecommendedArchiveDutyChoice.Undecided);
            dto.KLE = arg
                .TaskRefs
                .Select(taskRef => taskRef.MapIdentityNamePairDTO())
                .ToList();
        }
    }
}