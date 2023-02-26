using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Interface;
using System.Linq;

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
                ItInterfaceTypeUuid = itInterface.Interface?.MapIdentityNamePairDTO(),
                Data = itInterface.DataRows.Select(ToDTO).ToList()
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

        private static ItInterfaceDataResponseDTO ToDTO(DataRow arg)
        {
            return new ItInterfaceDataResponseDTO
            {
                DataType = arg.DataType?.MapIdentityNamePairDTO(),
                Uuid = arg.Uuid,
                Description = arg.Data
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