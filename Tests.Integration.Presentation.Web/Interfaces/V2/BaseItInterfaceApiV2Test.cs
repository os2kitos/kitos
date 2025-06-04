﻿using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Presentation.Web.Models.API.V2.Response.Interface;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.System;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Interfaces.V2
{
    public abstract class BaseItInterfaceApiV2Test : BaseTest
    {
        protected async Task<ShallowOrganizationResponseDTO> CreateOrganization(string cvr = "11223344")
        {
            return await CreateOrganizationAsync(cvr: cvr);
        }

        protected string CreateName()
        {
            return $"{nameof(ItInterfaceApiV2Test)}{A<string>()}";
        }

        protected string CreateLongString(string parameterName)
        {
            var longString = $"Too long {parameterName} parameter: ";
            while (longString.Length < ItInterface.MaxNameLength)
            {
                longString += A<string>();
            }
            return longString;
        }

        protected static void CheckBaseDTOValues(ItSystemResponseDTO system, ItInterfaceResponseDTO itInterface, BaseItInterfaceResponseDTO interfaceDTO)
        {
            Assert.Equal(itInterface.Name, interfaceDTO.Name);
            Assert.Equal(system?.Name, interfaceDTO.ExposedBySystem?.Name);
            Assert.Equal(system?.Uuid, interfaceDTO.ExposedBySystem?.Uuid);
            Assert.Equal(itInterface.Uuid, interfaceDTO.Uuid);
            Assert.Equal(itInterface.Description, interfaceDTO.Description);
            Assert.Equal(itInterface.Notes, interfaceDTO.Notes);
            Assert.Equal(itInterface.InterfaceId, interfaceDTO.InterfaceId);
            Assert.Equal(itInterface.UrlReference, interfaceDTO.UrlReference);
            Assert.Equal(itInterface.Version, interfaceDTO.Version);
        }

        protected static void BaseItInterfaceResponseDTODBCheck(ItInterface itInterface, BaseItInterfaceResponseDTO itInterfaceDTO)
        {
            Assert.Equal(itInterface.Uuid, itInterfaceDTO.Uuid);
            Assert.Equal(itInterface.Name, itInterfaceDTO.Name);
            Assert.Equal(itInterface.Description, itInterfaceDTO.Description);
            Assert.Equal(itInterface.ItInterfaceId, itInterfaceDTO.InterfaceId);
            Assert.Equal(itInterface.Url, itInterfaceDTO.UrlReference);
            Assert.Equal(itInterface.Version, itInterfaceDTO.Version);
            Assert.Equal(itInterface.Disabled, itInterfaceDTO.Deactivated);

            Assert.Equal(itInterface.Created, itInterfaceDTO.Created);
            Assert.Equal(itInterface.ObjectOwner.Uuid, itInterfaceDTO.CreatedBy.Uuid);
            Assert.Equal(itInterface.ObjectOwner.GetFullName(), itInterfaceDTO.CreatedBy.Name);

            Assert.Equal(itInterface.ExhibitedBy.ItSystem.Uuid, itInterfaceDTO.ExposedBySystem.Uuid);
            Assert.Equal(itInterface.ExhibitedBy.ItSystem.Name, itInterfaceDTO.ExposedBySystem.Name);
        }

        protected static void CheckCreatedInterface(CreateItInterfaceRequestDTO expected, ItInterfaceResponseDTO actual)
        {
            Assert.Equal(expected.OrganizationUuid, actual.OrganizationContext?.Uuid);
            CheckItInterface(expected, actual);
        }

        protected static void CheckItInterface(IItInterfaceWritablePropertiesRequestDTO expected, ItInterfaceResponseDTO actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.InterfaceId ?? "", actual.InterfaceId);
            Assert.Equal(expected.UrlReference, actual.UrlReference);
            Assert.Equal(expected.Version, actual.Version);
            Assert.Equal(expected.Deactivated, actual.Deactivated);
            Assert.Equal(expected.ExposedBySystemUuid, actual.ExposedBySystem?.Uuid);
            Assert.Equal(expected.ItInterfaceTypeUuid, actual.ItInterfaceType?.Uuid);
            Assert.Equal(expected.Scope, actual.Scope);
            Assert.Equivalent(expected.Data ?? new List<ItInterfaceDataRequestDTO>(), actual.Data.Select(ToItInterfaceDataRequestDto));
        }

        protected static ItInterfaceDataRequestDTO ToItInterfaceDataRequestDto(ItInterfaceDataResponseDTO x)
        {
            return new ItInterfaceDataRequestDTO()
            {
                Description = x.Description,
                DataTypeUuid = x.DataType?.Uuid
            };
        }
    }
}
