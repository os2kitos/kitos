using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response.Interface;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Interfaces.V2
{
    public class ItInterfaceApiV2Test : WithAutoFixture
    {

        [Fact]
        public async Task Can_Get_Interfaces_As_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface1.Id);
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface2.Id);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, org.Id, TestEnvironment.DefaultOrganizationId);

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfacesAsync(token, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, result.Count());
            var interface1DTO = result.Where(x => x.Name.Equals(itInterface1.Name)).First();
            CheckBaseDTOValues(system, itInterface1, interface1DTO);
        }

        [Fact]
        public async Task Can_Get_Interfaces_As_RightsHolder_From_Multiple_RightsHolders_If_RightsHolderAccess_In_Both()
        {
            //Arrange
            var (token, org1, org2) = await CreateRightsHolderUserInMultipleNewOrganizationsAsync();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system1.Id, itInterface1.Id);
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system2.Id, itInterface2.Id);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system1.Id, org1.Id, TestEnvironment.DefaultOrganizationId);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system2.Id, org2.Id, TestEnvironment.DefaultOrganizationId);

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfacesAsync(token, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, result.Count());
        }

        [Fact]
        public async Task Can_Get_Interfaces_From_Specific_RightsHolder()
        {
            //Arrange
            var (token, org1, org2) = await CreateRightsHolderUserInMultipleNewOrganizationsAsync();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system1.Id, itInterface1.Id);
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system2.Id, itInterface2.Id);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system1.Id, org1.Id, TestEnvironment.DefaultOrganizationId);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system2.Id, org2.Id, TestEnvironment.DefaultOrganizationId);

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

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, org.Id, TestEnvironment.DefaultOrganizationId);

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

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, org.Id, TestEnvironment.DefaultOrganizationId);

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ById(itInterface.Id);

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
                var dbInterface = x.AsQueryable().ById(itInterface.Id);
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
            var result = await InterfaceV2Helper.SendGetRightsholderInterfaceAsync(token, uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_Interface_As_RightsHolder_If_System_Has_No_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, null, TestEnvironment.DefaultOrganizationId);

            //Act
            var result = await InterfaceV2Helper.SendGetRightsholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_Interface_As_RightsHolder_If_System_Has_Different_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, TestEnvironment.SecondOrganizationId, TestEnvironment.DefaultOrganizationId);

            //Act
            var result = await InterfaceV2Helper.SendGetRightsholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Interface_As_Stakeholder_If_AccessModifier_Is_Public()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            CheckBaseDTOValues(system, itInterface, itInterfaceDTO);
        }

        [Fact]
        public async Task Can_Get_Interface_As_Stakeholder_If_Interface_In_Org_Where_User_Has_Any_Role()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), org.Id, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            CheckBaseDTOValues(system, itInterface, itInterfaceDTO);
        }

        [Fact]
        public async Task Cannot_Get_Interface_As_Stakeholder_If_AccessModifier_Is_Local()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);

            //Act
            var result = await InterfaceV2Helper.SendGetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Interface_As_Stakeholder_With_Correct_Data()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ById(itInterface.Id);

                dbInterface.Description = A<string>();
                dbInterface.ItInterfaceId = A<string>();
                dbInterface.Name = A<string>();
                dbInterface.Url = A<string>();
                dbInterface.Version = A<string>().Substring(0, ItInterface.MaxVersionLength); // Version has maxLength of 20
                dbInterface.Disabled = A<bool>();

                db.SaveChanges();
            });

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.NotNull(itInterfaceDTO);
            DatabaseAccess.MapFromEntitySet<ItInterface, bool>(x =>
            {
                var dbInterface = x.AsQueryable().ById(itInterface.Id);
                BaseItInterfaceResponseDTODBCheck(dbInterface, itInterfaceDTO);

                Assert.Equal(dbInterface.LastChanged, itInterfaceDTO.LastModified);
                Assert.Equal(dbInterface.LastChanged, itInterfaceDTO.LastModified);

                Assert.Equal(dbInterface.LastChangedByUser.Uuid, itInterfaceDTO.LastModifiedBy.Uuid);
                Assert.Equal(dbInterface.LastChangedByUser.GetFullName(), itInterfaceDTO.LastModifiedBy.Name);

                return true;
            });
        }

        [Fact]
        public async Task Can_Get_Interfaces_As_Stakeholder()
        {
            //Arrange - Making sure there are at least 2 public interfaces
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface1.Id);
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface2.Id);

            //Act
            var result = await InterfaceV2Helper.GetStakeholderInterfacesAsync(token, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, result.Count());
        }

        [Fact]
        public async Task Can_Get_Interfaces_As_Stakeholder_By_Exposing_System()
        {
            //Arrange - Making sure there are at least 2 public interfaces
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system1.Id, itInterface1.Id);
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system2.Id, itInterface2.Id);

            //Act
            var result = await InterfaceV2Helper.GetStakeholderInterfacesAsync(token, pageSize, pageNumber, system1.Uuid);

            //Assert
            Assert.Single(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Post_ItInterface_As_Rightsholder(bool withProvidedUuid)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var exposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

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
            var exposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = withoutRightsHolder ? Guid.Empty : org.Uuid,
                ExposedBySystemUuid = withoutExposingSystem ? Guid.Empty : exposingSystem.Uuid,
                Name = withoutName ? null : A<string>(),
                Description = wihtoutDescription ? null : A<string>(),
                UrlReference = withoutUrlReference ? null : A<string>()
            };

            //Act
            var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createdInterface.StatusCode);
        }

        [Fact]
        public async Task Cannot_Post_ItInterface_As_Rightsholder_To_Organization_Where_User_Is_Not_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync(); 
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);
            var exposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = defaultOrgUuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act
            var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdInterface.StatusCode);
        }

        [Fact]
        public async Task Cannot_Post_ItInterface_As_Rightsholder_With_ExposingSystem_Where_User_Does_Not_Have_Read_Access()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var exposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = org.Uuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act
            var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

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
            var exposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

            var input = new RightsHolderCreateItInterfaceRequestDTO()
            {
                RightsHolderUuid = org.Uuid,
                ExposedBySystemUuid = exposingSystem.Uuid,
                Name = withLongName ? CreateLongString("name") : A<string>(),
                Description = withLongItInterfaceId ? CreateLongString("description") : A<string>(),
                UrlReference = A<string>(),
                Version = withLongVersion ? CreateLongString("version") : A<string>().Substring(0, 20)
            };

            //Act
            var createdInterface = await InterfaceV2Helper.SendCreateRightsHolderItInterfaceAsync(token, input);

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

            var newExposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

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

            //Act
            using var response = await InterfaceV2Helper.SendUpdateRightsHolderItInterfaceAsync(token, A<Guid>(), A<RightsHolderWritableItInterfacePropertiesDTO>());

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Put_ItInterface_As_Rightsholder_If_ItInterface_Does_Not_Exist()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var exposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

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
        [InlineData( true, true, true, false)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, true, true, true)]
        public async Task Cannot_Put_ItInterface_As_Rightsholder_Without_All_Required_Fields(bool withoutExposingSystem, bool withoutName, bool wihtoutDescription, bool withoutUrlReference)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();

            var creationDTO = await CreateRightsHolderItInterfaceRequestDTO(false, org);
            var createdInterface = await InterfaceV2Helper.CreateRightsHolderItInterfaceAsync(token, creationDTO);

            var newExposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

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

            var newExposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

            var input = new RightsHolderWritableItInterfacePropertiesDTO()
            {
                ExposedBySystemUuid = newExposingSystem.Uuid,
                Name = withLongName ? CreateLongString("name") : A<string>(),
                Description = withLongItInterfaceId ? CreateLongString("description") : A<string>(),
                UrlReference = A<string>(),
                Version = withLongVersion ? CreateLongString("version") : A<string>().Substring(0, 20)
            };

            //Act
            using var response = await InterfaceV2Helper.SendUpdateRightsHolderItInterfaceAsync(token, createdInterface.Uuid, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private async Task<RightsHolderCreateItInterfaceRequestDTO> CreateRightsHolderItInterfaceRequestDTO(bool withProvidedUuid, Organization rightsHolderOrganization)
        {
            var exposingSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), rightsHolderOrganization.Id, AccessModifier.Public);

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

        private async Task<(string token, Organization createdOrganization)> CreateStakeHolderUserInNewOrg()
        {
            var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, org.Id, true, true);
            return (token, org);
        }

        private async Task<(string token, Organization createdOrganization)> CreateRightsHolderUserInNewOrganizationAsync()
        {
            var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.RightsHolderAccess, org.Id, true);
            return (token, org);
        }

        private async Task<(string token, Organization createdOrganization1, Organization createdOrganization2)> CreateRightsHolderUserInMultipleNewOrganizationsAsync()
        {
            var org1 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var org2 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);

            var (userId, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.RightsHolderAccess, org1.Id, true);
            await HttpApi.SendAssignRoleToUserAsync(userId, OrganizationRole.RightsHolderAccess, org2.Id);

            return (token, org1, org2);
        }

        private string CreateName()
        {
            return $"{nameof(ItInterfaceApiV2Test)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{A<string>()}@test.dk";
        }

        private string CreateLongString(string parameterName)
        {
            var longString = $"Too long {parameterName} parameter: ";
            while (longString.Length < ItInterface.MaxNameLength)
            {
                longString += A<string>();
            }
            return longString;
        }

        private static void CheckBaseDTOValues(ItSystemDTO system, ItInterfaceDTO itInterface, BaseItInterfaceResponseDTO interfaceDTO)
        {
            Assert.Equal(itInterface.Name, interfaceDTO.Name);
            Assert.Equal(system.Name, interfaceDTO.ExposedBySystem.Name);
            Assert.Equal(system.Uuid, interfaceDTO.ExposedBySystem.Uuid);
            Assert.Equal(itInterface.Uuid, interfaceDTO.Uuid);
            Assert.Equal(itInterface.Description, interfaceDTO.Description);
            Assert.Equal(itInterface.ItInterfaceId, interfaceDTO.InterfaceId);
            Assert.Equal(itInterface.Url, interfaceDTO.UrlReference);
            Assert.Equal(itInterface.Version, interfaceDTO.Version);
        }

        private static void BaseItInterfaceResponseDTODBCheck(ItInterface itInterface, BaseItInterfaceResponseDTO itInterfaceDTO)
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
    }
}
