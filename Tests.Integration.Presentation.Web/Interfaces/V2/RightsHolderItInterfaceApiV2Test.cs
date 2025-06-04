using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V2.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;

namespace Tests.Integration.Presentation.Web.Interfaces.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class RightsHolderItInterfaceApiV2Test : BaseItInterfaceApiV2Test
    {
        [Fact]
        public async Task Can_Get_Interfaces_As_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface1.Uuid, system.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface2.Uuid, system.Uuid);

            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, org.Uuid).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfacesAsync(token, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, result.Count());
            var interface1DTO = result.First(x => x.Name.Equals(itInterface1.Name));
            CheckBaseDTOValues(system, itInterface1, interface1DTO);
            var interface2DTO = result.First(x => x.Name.Equals(itInterface2.Name));
            CheckBaseDTOValues(system, itInterface2, interface2DTO);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Get_Interfaces_As_RightsHolder_Depends_On_IncludeDeactivated(bool shouldIncludeDeactivated)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface1.Uuid, system.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface2.Uuid, system.Uuid);

            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, org.Uuid).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            // Disable second interface
            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ByUuid(itInterface2.Uuid);
                dbInterface.Disabled = true;
                db.SaveChanges();
            });

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfacesAsync(token, pageSize, pageNumber, includeDeactivated: shouldIncludeDeactivated);

            //Assert
            if (shouldIncludeDeactivated)
            {
                Assert.Equal(pageSize, result.Count());
                var interface1DTO = result.First(x => x.Name.Equals(itInterface1.Name));
                CheckBaseDTOValues(system, itInterface1, interface1DTO);
                Assert.False(interface1DTO.Deactivated);
                var interface2DTO = result.First(x => x.Name.Equals(itInterface2.Name));
                CheckBaseDTOValues(system, itInterface2, interface2DTO);
                Assert.True(interface2DTO.Deactivated);
            }
            else
            {
                var interfaceDTO = Assert.Single(result);
                Assert.False(interfaceDTO.Deactivated);
                CheckBaseDTOValues(system, itInterface1, interfaceDTO);
            }
        }

        [Fact]
        public async Task Can_Get_Interfaces_As_RightsHolder_From_Multiple_RightsHolders_If_RightsHolderAccess_In_Both()
        {
            //Arrange
            var (token, org1, org2) = await CreateRightsHolderUserInMultipleNewOrganizationsAsync();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system1 = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var system2 = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface1.Uuid, system1.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface2.Uuid, system2.Uuid);


            await ItSystemV2Helper.PatchRightsHolderAsync(system1.Uuid, org1.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItSystemV2Helper.PatchRightsHolderAsync(system2.Uuid, org2.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfacesAsync(token, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, result.Count());
        }

        [Fact]
        public async Task GET_Many_As_RightsHolder_With_ChangesSince_Filter()
        {
            //Arrange
            var (token, organization) = await CreateRightsHolderUserInNewOrganizationAsync();
            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface3 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);

            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, organization.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Change all in a specific order
            foreach (var interfaceDto in new[] { itInterface2, itInterface3, itInterface1 })
            {
                await InterfaceV2Helper.PatchExposedBySystemAsync(interfaceDto.Uuid, system.Uuid);
            }

            var interface3LastModified = DatabaseAccess.MapFromEntitySet<ItInterface, DateTime>(x => x.AsQueryable().ByUuid(itInterface3.Uuid).LastChanged.Transform(dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc)));

            //Act
            var dtos = (await InterfaceV2Helper.GetRightsholderInterfacesAsync(token, changedSinceGtEq: interface3LastModified, pageNumber: 0, pageSize: 10)).ToList();

            //Assert that the right interfaces are returned in the correct order
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { itInterface3.Uuid, itInterface1.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        [Fact]
        public async Task Can_Get_Interfaces_From_Specific_RightsHolder()
        {
            //Arrange
            var (token, org1, org2) = await CreateRightsHolderUserInMultipleNewOrganizationsAsync();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system1 = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var system2 = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface2 = await CreateItInterfaceAsync(org1.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface1.Uuid, system1.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface2.Uuid, system2.Uuid);
            await ItSystemV2Helper.PatchRightsHolderAsync(system1.Uuid, org1.Uuid).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItSystemV2Helper.PatchRightsHolderAsync(system2.Uuid, org2.Uuid).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfacesAsync(token, pageSize, pageNumber, org1.Uuid);

            //Assert
            var interface1DTO = Assert.Single(result);
            CheckBaseDTOValues(system1, itInterface1, interface1DTO);
        }

        [Fact]
        public async Task Can_Get_Interface_As_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface.Uuid, system.Uuid);
            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, org.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            CheckBaseDTOValues(system, itInterface, result);
        }

        [Fact]
        public async Task Can_Get_Interface_As_RightsHolder_With_Correct_Data()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface.Uuid, system.Uuid);
            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, org.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ByUuid(itInterface.Uuid);

                dbInterface.Description = A<string>();
                dbInterface.ItInterfaceId = A<string>();
                dbInterface.Name = A<string>();
                dbInterface.Url = A<string>();
                dbInterface.Version = A<string>().Substring(0, ItInterface.MaxVersionLength); // Version has maxLength of 20
                dbInterface.Disabled = A<bool>();

                db.SaveChanges();
            });

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetRightsholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.NotNull(itInterfaceDTO);
            DatabaseAccess.MapFromEntitySet<ItInterface, bool>(x =>
            {
                var dbInterface = x.AsQueryable().ByUuid(itInterface.Uuid);
                BaseItInterfaceResponseDTODBCheck(dbInterface, itInterfaceDTO);

                return true;
            });
        }



        [Fact]
        public async Task Cannot_Get_Interface_As_RightsHolder_If_Not_Exists()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var uuid = A<Guid>();

            //Act
            using var result = await InterfaceV2Helper.SendGetRightsholderInterfaceAsync(token, uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_Interface_As_RightsHolder_If_System_Has_No_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface.Uuid, system.Uuid);
            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, null).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Act
            using var result = await InterfaceV2Helper.SendGetRightsholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_Interface_As_RightsHolder_If_System_Has_Different_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface.Uuid, system.Uuid);
            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.SecondOrganizationId));

            //Act
            using var result = await InterfaceV2Helper.SendGetRightsholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Post_ItInterface_As_Rightsholder(bool withProvidedUuid)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var exposingSystem = await CreateItSystemAsync(org.Uuid);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = org.Uuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = A<string>(),
                InterfaceId = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>(),
                Version = A<string>().Substring(0, 20),
                Uuid = withProvidedUuid ? A<Guid>() : null
            };

            //Act
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, input);

            //Assert
            if (withProvidedUuid)
                Assert.Equal(input.Uuid, createdInterface.Uuid);
            else
                Assert.NotEqual(Guid.Empty, createdInterface.Uuid);

            Assert.Equal(input.ExposedBySystemUuid, createdInterface.ExposedBySystem.Uuid);
            Assert.Equal(input.Name, createdInterface.Name);
            Assert.Equal(input.InterfaceId, createdInterface.InterfaceId);
            Assert.Equal(input.Description, createdInterface.Description);
            Assert.Equal(input.UrlReference, createdInterface.UrlReference);
            Assert.Equal(input.Version, createdInterface.Version);
        }

        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(true, true, true, true, false)]
        [InlineData(true, true, true, false, true)]
        [InlineData(true, true, false, true, true)]
        [InlineData(true, false, true, true, true)]
        [InlineData(false, true, true, true, true)]
        public async Task Cannot_Post_ItInterface_As_Rightsholder_Without_All_Required_Fields(bool withoutRightsHolder, bool withoutExposingSystem, bool withoutName, bool wihtoutDescription, bool withoutUrlReference)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var exposingSystem = await CreateItSystemAsync(org.Uuid);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = withoutRightsHolder ? Guid.Empty : org.Uuid,
                ExposedBySystemUuid = withoutExposingSystem ? Guid.Empty : exposingSystem.Uuid,
                Name = withoutName ? null : A<string>(),
                Description = wihtoutDescription ? null : A<string>(),
                UrlReference = withoutUrlReference ? null : A<string>()
            };

            //Act
            using var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createdInterface.StatusCode);
        }

        [Fact]
        public async Task Cannot_Post_ItInterface_As_Rightsholder_To_Organization_Where_User_Is_Not_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var exposingSystem = await CreateItSystemAsync(org.Uuid);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = DefaultOrgUuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act
            using var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdInterface.StatusCode);
        }

        [Fact]
        public async Task Cannot_Post_ItInterface_As_Rightsholder_With_ExposingSystem_Where_User_Does_Not_Have_Read_Access()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var otherOrg = await CreateOrganization();
            var exposingSystem = await CreateItSystemAsync(otherOrg.Uuid);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = org.Uuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act
            using var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdInterface.StatusCode);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public async Task Cannot_Post_ItInterface_As_Rightsholder_With_Parameter_Being_Longer_Than_MaxLength(bool withLongVersion, bool withLongName, bool withLongItInterfaceId)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var exposingSystem = await CreateItSystemAsync(org.Uuid);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = org.Uuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = withLongName ? CreateLongString("name") : A<string>(),
                Description = A<string>(),
                InterfaceId = withLongItInterfaceId ? CreateLongString("interface id") : A<string>(),
                UrlReference = A<string>(),
                Version = withLongVersion ? CreateLongString("version") : A<string>().Substring(0, 20)
            };

            //Act
            using var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createdInterface.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Put_ItInterface_As_Rightsholder(bool withProvidedUuid)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(withProvidedUuid, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var newExposingSystem = await CreateItSystemAsync(org.Uuid);

            var updateParameters = new RightsHolderWritableItInterfacePropertiesDTO()
            {
                ExposedBySystemUuid = newExposingSystem.Uuid,
                Name = A<string>(),
                InterfaceId = A<string>(),
                Version = A<string>().Substring(0, 20),
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act
            var updatedInterface = await InterfaceV2Helper.UpdateRightsHolderItInterfaceAsync(token, createdInterface.Uuid, updateParameters);

            //Assert
            Assert.Equal(createdInterface.Uuid, updatedInterface.Uuid); // Make sure Uuid has not been changed
            Assert.Equal(updateParameters.ExposedBySystemUuid, updatedInterface.ExposedBySystem.Uuid);
            Assert.Equal(updateParameters.Name, updatedInterface.Name);
            Assert.Equal(updateParameters.InterfaceId, updatedInterface.InterfaceId);
            Assert.Equal(updateParameters.Description, updatedInterface.Description);
            Assert.Equal(updateParameters.UrlReference, updatedInterface.UrlReference);
            Assert.Equal(updateParameters.Version, updatedInterface.Version);
        }

        [Fact]
        public async Task Cannot_Put_ItInterface_As_Rightsholder_If_ExposingSystem_Does_Not_Exist()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var updateRequest = new RightsHolderWritableItInterfacePropertiesDTO
            {
                ExposedBySystemUuid = A<Guid>()
            };

            //Act
            using var response = await InterfaceV2Helper.SendUpdateRightsHolderItInterfaceAsync(token, createdInterface.Uuid, updateRequest);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Put_ItInterface_As_Rightsholder_If_ItInterface_Does_Not_Exist()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var exposingSystem = await CreateItSystemAsync(org.Uuid);

            var updateParameters = A<RightsHolderWritableItInterfacePropertiesDTO>();
            updateParameters.ExposedBySystemUuid = exposingSystem.Uuid;

            //Act
            using var response = await InterfaceV2Helper.SendUpdateRightsHolderItInterfaceAsync(token, A<Guid>(), updateParameters);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Put_ItInterface_As_Rightsholder_To_ItInterface_With_Wrong_Rightsholder()
        {
            //Arrange
            var (token1, org1) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org1);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token1, creationDTO);

            var updateParameters = new RightsHolderWritableItInterfacePropertiesDTO()
            {
                ExposedBySystemUuid = createdInterface.ExposedBySystem.Uuid,
                Name = A<string>(),
                InterfaceId = A<string>(),
                Version = A<string>().Substring(0, 20),
                Description = A<string>(),
                UrlReference = A<string>()
            };

            var (token2, org2) = await CreateRightsHolderUserInNewOrganizationAsync();

            //Act
            using var response = await InterfaceV2Helper.SendUpdateRightsHolderItInterfaceAsync(token2, createdInterface.Uuid, updateParameters);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, true, true, true)]
        public async Task Cannot_Put_ItInterface_As_Rightsholder_Without_All_Required_Fields(bool withoutExposingSystem, bool withoutName, bool wihtoutDescription, bool withoutUrlReference)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var newExposingSystem = await CreateItSystemAsync(org.Uuid);

            var input = new RightsHolderWritableItInterfacePropertiesDTO()
            {
                ExposedBySystemUuid = withoutExposingSystem ? Guid.Empty : newExposingSystem.Uuid,
                Name = withoutName ? null : A<string>(),
                Description = wihtoutDescription ? null : A<string>(),
                UrlReference = withoutUrlReference ? null : A<string>()
            };

            //Act
            using var response = await InterfaceV2Helper.SendUpdateRightsHolderItInterfaceAsync(token, createdInterface.Uuid, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public async Task Cannot_Put_ItInterface_As_Rightsholder_With_Parameter_Being_Longer_Than_MaxLength(bool withLongVersion, bool withLongName, bool withLongItInterfaceId)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var newExposingSystem = await CreateItSystemAsync(org.Uuid);

            var input = new RightsHolderWritableItInterfacePropertiesDTO()
            {
                ExposedBySystemUuid = newExposingSystem.Uuid,
                Name = withLongName ? CreateLongString("name") : A<string>(),
                Description = A<string>(),
                InterfaceId = withLongItInterfaceId ? CreateLongString("interface id") : A<string>(),
                UrlReference = A<string>(),
                Version = withLongVersion ? CreateLongString("version") : A<string>().Substring(0, 20)
            };

            //Act
            using var response = await InterfaceV2Helper.SendUpdateRightsHolderItInterfaceAsync(token, createdInterface.Uuid, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true)]
        [InlineData(true, true, true, true, true, false)]
        [InlineData(true, true, true, true, false, true)]
        [InlineData(true, true, true, false, true, true)]
        [InlineData(true, true, false, true, true, true)]
        [InlineData(true, false, true, true, true, true)]
        [InlineData(false, true, true, true, true, true)]
        [InlineData(false, false, false, false, false, false)]
        public async Task Can_Patch_ItInterface_As_Rightsholder(
            bool withName,
            bool withInterfaceId,
            bool withExposedBySystem,
            bool withVersion,
            bool withDescription,
            bool withUrlReference)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var changes = new Dictionary<string, object>();
            if (withName) changes.Add(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Name), CreateName());
            if (withInterfaceId) changes.Add(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.InterfaceId), A<string>());
            if (withExposedBySystem)
            {
                var newExposingSystem = await CreateItSystemAsync(org.Uuid);
                changes.Add(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.ExposedBySystemUuid), newExposingSystem.Uuid);
            }
            if (withVersion) changes.Add(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Version), A<string>().Substring(0, 20));
            if (withDescription) changes.Add(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Description), A<string>());
            if (withUrlReference) changes.Add(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.UrlReference), A<string>());

            //Act
            var updatedInterface = await InterfaceV2Helper.PatchRightsHolderInterfaceAsync(token, createdInterface.Uuid, changes.ToArray());

            //Assert
            Assert.Equal(withName ? changes[nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Name)] : createdInterface.Name, updatedInterface.Name);
            Assert.Equal(withInterfaceId ? changes[nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.InterfaceId)] : createdInterface.InterfaceId, updatedInterface.InterfaceId);
            Assert.Equal(withExposedBySystem ? changes[nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.ExposedBySystemUuid)] : createdInterface.ExposedBySystem.Uuid, updatedInterface.ExposedBySystem.Uuid);
            Assert.Equal(withVersion ? changes[nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Version)] : createdInterface.Version, updatedInterface.Version);
            Assert.Equal(withDescription ? changes[nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Description)] : createdInterface.Description, updatedInterface.Description);
            Assert.Equal(withUrlReference ? changes[nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.UrlReference)] : createdInterface.UrlReference, updatedInterface.UrlReference);
        }

        [Fact]
        public async Task Delete_As_RightsHolder_Deactivates_System()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await InterfaceV2Helper.SendDeleteRightsHolderItInterfaceAsync(token, createdInterface.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var dto = await InterfaceV2Helper.GetRightsholderInterfaceAsync(token, createdInterface.Uuid);
            Assert.True(dto.Deactivated);
        }

        [Fact]
        public async Task Cannot_Delete_As_RightsHolder_Without_Reason()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var reason = new DeactivationReasonRequestDTO() { DeactivationReason = string.Empty };

            //Act
            using var result = await InterfaceV2Helper.SendDeleteRightsHolderItInterfaceAsync(token, createdInterface.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var dto = await InterfaceV2Helper.GetRightsholderInterfaceAsync(token, createdInterface.Uuid);
            Assert.False(dto.Deactivated); //Deactivation should not have happened
        }

        [Fact]
        public async Task Cannot_Delete_As_RightsHolder_Without_Access()
        {
            //Arrange
            var (token1, org1) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org1);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token1, creationDTO);

            var (token2, org2) = await CreateRightsHolderUserInNewOrganizationAsync();

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await InterfaceV2Helper.SendDeleteRightsHolderItInterfaceAsync(token2, createdInterface.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_Unknown_System()
        {
            //Arrange

            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var uuid = A<Guid>();

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await InterfaceV2Helper.SendDeleteRightsHolderItInterfaceAsync(token, uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Delete_As_RightsHolder_If_Already_Deactivated()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);
            DatabaseAccess.MutateEntitySet<ItInterface>(repository => repository.AsQueryable().ByUuid(createdInterface.Uuid).Deactivate());

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await InterfaceV2Helper.SendDeleteRightsHolderItInterfaceAsync(token, createdInterface.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Invoke_Endpoint_Blocked_From_rightsHolders()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            //Act
            using var result = await InterfaceV2Helper.SendGetInterfacesAsync(token);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        private async Task<RightsHolderCreateItInterfaceRequestDTO> CreateRightsHolderItInterfaceRequestDTO(bool withProvidedUuid, ShallowOrganizationResponseDTO rightsHolderOrganization)
        {
            var exposingSystem = await CreateItSystemAsync(DefaultOrgUuid, rightsHolderUuid: rightsHolderOrganization.Uuid);

            return new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = rightsHolderOrganization.Uuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = A<string>(),
                InterfaceId = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>(),
                Version = A<string>().Substring(0, 20),
                Uuid = withProvidedUuid ? A<Guid>() : null
            };
        }

        private async Task<(string token, ShallowOrganizationResponseDTO createdOrganization)> CreateRightsHolderUserInNewOrganizationAsync()
        {
            var org = await CreateOrganizationAsync();
            var (userId, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.RightsHolderAccess, org.Uuid, true);
            return (token, org);
        }

        private async Task<(string token, ShallowOrganizationResponseDTO createdOrganization1, ShallowOrganizationResponseDTO createdOrganization2)> CreateRightsHolderUserInMultipleNewOrganizationsAsync()
        {
            var org1 = await CreateOrganization();
            var org2 = await CreateOrganization();

            var (userId, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.RightsHolderAccess, org1.Uuid, true);
            await HttpApi.SendAssignRoleToUserAsync(userId, OrganizationRole.RightsHolderAccess, org2.Uuid).DisposeAsync();

            return (token, org1, org2);
        }
    }
}
