using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Interface;
using System.Linq;
using Core.ApplicationServices.Model.Interface;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping
{
    public class ItInterfaceResponseMapper : IItInterfaceResponseMapper
    {
        public ItInterfaceResponseDTO ToItInterfaceResponseDTO(ItInterface itInterface)
        {
            var dto = new ItInterfaceResponseDTO
            {
                LastModified = itInterface.LastChanged,
                LastModifiedBy = itInterface.LastChangedByUser?.MapIdentityNamePairDTO(),
                Scope = itInterface.AccessModifier.ToChoice(),
                ItInterfaceType = itInterface.Interface?.MapIdentityNamePairDTO(),
                Data = itInterface.DataRows.Select(ToDataResponseDTO).ToList(),
                OrganizationContext = itInterface.Organization?.MapShallowOrganizationResponseDTO(),
                RightsHolder = itInterface.GetRightsHolderOrganization()?.MapShallowOrganizationResponseDTO()
            };
            MapBaseInformation(itInterface, dto);
            return dto;
        }

        public RightsHolderItInterfaceResponseDTO ToRightsHolderItInterfaceResponseDTO(ItInterface itInterface)
        {
            var dto = new RightsHolderItInterfaceResponseDTO();
            MapBaseInformation(itInterface, dto);
            return dto;
        }

        public ItInterfaceDataResponseDTO ToDataResponseDTO(DataRow row)
        {
            return new ItInterfaceDataResponseDTO
            {
                DataType = row.DataType?.MapIdentityNamePairDTO(),
                Uuid = row.Uuid,
                Description = row.Data
            };
        }

        public ItInterfacePermissionsResponseDTO Map(ItInterfacePermissions permissions)
        {
            return new ItInterfacePermissionsResponseDTO()
            {
                Delete = permissions.BasePermissions.Delete,
                Read = permissions.BasePermissions.Read,
                Modify = permissions.BasePermissions.Modify,
                DeletionConflicts = permissions.DeletionConflicts.Select(x => x.ToChoice()).ToList()
            };
        }

        private static void MapBaseInformation<T>(ItInterface input, T outputDTO) where T : BaseItInterfaceResponseDTO
        {
            outputDTO.Uuid = input.Uuid;
            outputDTO.ExposedBySystem = input.ExhibitedBy?.ItSystem?.MapIdentityNamePairDTO();
            outputDTO.Name = input.Name;
            outputDTO.InterfaceId = input.ItInterfaceId;
            outputDTO.Version = input.Version;
            outputDTO.Description = input.Description;
            outputDTO.Notes = input.Note;
            outputDTO.UrlReference = input.Url;
            outputDTO.Deactivated = input.Disabled;
            outputDTO.Created = input.Created;
            outputDTO.CreatedBy = input.ObjectOwner?.MapIdentityNamePairDTO();
        }
    }
}