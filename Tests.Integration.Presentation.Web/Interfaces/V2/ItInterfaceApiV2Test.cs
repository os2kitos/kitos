using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models;
using Presentation.Web.Models.External.V2.Response;
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
            CheckDTOValues(system, itInterface1, interface1DTO);
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
        public async Task Can_Get_Interfaces_As_RightsHolder_From_Specific_RightsHolder()
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
            CheckDTOValues(system1, itInterface1, interface1DTO);
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
            CheckDTOValues(system, itInterface, result);
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

                Assert.Equal(dbInterface.Uuid, itInterfaceDTO.Uuid);
                Assert.Equal(dbInterface.Name, itInterfaceDTO.Name);
                Assert.Equal(dbInterface.Description, itInterfaceDTO.Description);
                Assert.Equal(dbInterface.ItInterfaceId, itInterfaceDTO.InterfaceId);
                Assert.Equal(dbInterface.Url, itInterfaceDTO.UrlReference);
                Assert.Equal(dbInterface.Version, itInterfaceDTO.Version);
                Assert.Equal(dbInterface.Disabled, itInterfaceDTO.Deactivated);

                Assert.Equal(dbInterface.Created, itInterfaceDTO.Created);
                Assert.Equal(dbInterface.ObjectOwner.Uuid, itInterfaceDTO.CreatedBy.Uuid);
                Assert.Equal(dbInterface.ObjectOwner.GetFullName(), itInterfaceDTO.CreatedBy.Name);

                Assert.Equal(dbInterface.ExhibitedBy.ItSystem.Uuid, itInterfaceDTO.ExposedBySystem.Uuid);
                Assert.Equal(dbInterface.ExhibitedBy.ItSystem.Name, itInterfaceDTO.ExposedBySystem.Name);

                return true;
            });
        }

        [Fact]
        public async Task Cannot_Get_Interface_As_RightsHolder_If_Not_Exists()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderUserInNewOrganizationAsync();
            var uuid = Guid.NewGuid();

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
            CheckDTOValues(system, itInterface, itInterfaceDTO);
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
            CheckDTOValues(system, itInterface, itInterfaceDTO);
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

                Assert.Equal(dbInterface.Uuid, itInterfaceDTO.Uuid);
                Assert.Equal(dbInterface.Name, itInterfaceDTO.Name);
                Assert.Equal(dbInterface.Description, itInterfaceDTO.Description);
                Assert.Equal(dbInterface.ItInterfaceId, itInterfaceDTO.InterfaceId);
                Assert.Equal(dbInterface.Url, itInterfaceDTO.UrlReference);
                Assert.Equal(dbInterface.Version, itInterfaceDTO.Version);
                Assert.Equal(dbInterface.Disabled, itInterfaceDTO.Deactivated);

                Assert.Equal(dbInterface.Created, itInterfaceDTO.Created);
                Assert.Equal(dbInterface.ObjectOwner.Uuid, itInterfaceDTO.CreatedBy.Uuid);
                Assert.Equal(dbInterface.ObjectOwner.GetFullName(), itInterfaceDTO.CreatedBy.Name);

                Assert.Equal(dbInterface.ExhibitedBy.ItSystem.Uuid, itInterfaceDTO.ExposedBySystem.Uuid);
                Assert.Equal(dbInterface.ExhibitedBy.ItSystem.Name, itInterfaceDTO.ExposedBySystem.Name);

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

        private static void CheckDTOValues(ItSystemDTO system, ItInterfaceDTO itInterface, ItInterfaceResponseDTO interfaceDTO)
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
    }
}
