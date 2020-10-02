using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Shared;
using Core.DomainModel.Organization;
using ExpectedObjects;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR
{
    public class DataProcessingRegistrationsTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Create()
        {
            //Arrange
            var name = A<string>();

            //Act
            var response = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Can_Create_With_Same_Name_In_Different_Organizations()
        {
            //Arrange
            var name = A<string>();

            //Act
            var responseInFirstOrg = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var responseInSecondOrg = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.SecondOrganizationId, name).ConfigureAwait(false);

            //Assert
            Assert.NotNull(responseInFirstOrg);
            Assert.NotNull(responseInSecondOrg);
            Assert.NotEqual(responseInFirstOrg.Id, responseInSecondOrg.Id);
            Assert.Equal(responseInFirstOrg.Name, responseInSecondOrg.Name);
        }

        [Fact]
        public async Task Cannot_Create_With_Duplicate_Name_In_Same_Organization()
        {
            //Arrange
            var name = A<string>();
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act
            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name).ConfigureAwait(false);
            using var secondResponse = await DataProcessingRegistrationHelper.SendCreateRequestAsync(organizationId, name).ConfigureAwait(false);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Get()
        {
            //Arrange
            var name = A<string>();
            var dto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Act
            var gotten = await DataProcessingRegistrationHelper.GetAsync(dto.Id);

            //Assert
            Assert.NotNull(gotten);
            dto.ToExpectedObject().ShouldMatch(gotten);
        }

        [Fact]
        public async Task Can_Delete()
        {
            //Arrange
            var name = A<string>();
            var dto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Act
            using var deleteResponse = await DataProcessingRegistrationHelper.SendDeleteRequestAsync(dto.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Change_Name()
        {
            //Arrange
            var name1 = A<string>();
            var name2 = A<string>();
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name1).ConfigureAwait(false);

            //Act
            using var response = await DataProcessingRegistrationHelper.SendChangeNameRequestAsync(registrationDto.Id, name2);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Change_Name_To_NonUniqueName_In_Same_Org()
        {
            //Arrange
            var name1 = A<string>();
            var name2 = A<string>();
            await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name1).ConfigureAwait(false);
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name2).ConfigureAwait(false);

            //Act
            using var response = await DataProcessingRegistrationHelper.SendChangeNameRequestAsync(registrationDto.Id, name1);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Can_Create_Endpoint_Returns_Ok()
        {
            //Arrange
            var name = A<string>();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendCanCreateRequestAsync(TestEnvironment.DefaultOrganizationId, name);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Create_Endpoint_Returns_Error_If_Non_Unique()
        {
            //Arrange
            var name = A<string>();
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name).ConfigureAwait(false);

            //Act
            using var response = await DataProcessingRegistrationHelper.SendCanCreateRequestAsync(organizationId, name);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData("")] //too short
        [InlineData("    ")] //only whitespace
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")] //101 chars
        public async Task Can_Create_Returns_Error_If_InvalidName(string name)
        {
            //Act
            using var response = await DataProcessingRegistrationHelper.SendCanCreateRequestAsync(TestEnvironment.DefaultOrganizationId, name);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Available_Roles()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());

            //Act
            var roles = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);

            //Assert
            Assert.NotEmpty(roles);
        }

        [Fact]
        public async Task Can_Get_Available_Users()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();

            //Act
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);

            //Assert
            Assert.NotEmpty(availableUsers);
        }

        [Fact]
        public async Task Can_Assign_Role()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);
            var user = availableUsers.First();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            registration = await DataProcessingRegistrationHelper.GetAsync(registration.Id);

            //Assert role is in the DTO
            var assignedRoleDto = Assert.Single(registration.AssignedRoles);
            Assert.Equal(user.Id, assignedRoleDto.User.Id);
            Assert.Equal(role.Id, assignedRoleDto.Role.Id);

            //Assert query endpoint now excludes possible duplicate
            availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);
            Assert.Empty(availableUsers.Where(x => x.Id == user.Id));
        }

        [Fact]
        public async Task Cannot_Assign_Duplicate_Role()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);
            var user = availableUsers.First();
            using var succeededResponse = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, role.Id, user.Id);

            //Act
            using var duplicateResponse = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.OK, succeededResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_Assign_Role_To_User_Not_In_Organization()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, role.Id, int.MaxValue);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Assign_UnAvailable_Role()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());

            //Act
            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, int.MaxValue, TestEnvironment.DefaultUserId);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_Remove_Role()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);
            var user = availableUsers.First();
            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(registration.Id, role.Id, user.Id);

            //Act
            using var removeResponse = await DataProcessingRegistrationHelper.SendRemoveRoleRequestAsync(registration.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);

            //Assert that the role is no longer in the DTO
            registration = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            Assert.Empty(registration.AssignedRoles);
        }

        [Fact]
        public async Task Cannot_Remove_Unassigned_Role()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(registration.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(registration.Id, role.Id);
            var user = availableUsers.First();

            //Act - check on an registration where no role has been added yet
            using var removeResponse = await DataProcessingRegistrationHelper.SendRemoveRoleRequestAsync(registration.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, removeResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Set_Master_Reference()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var reference = await ReferencesHelper.CreateReferenceAsync(A<string>(), A<string>(), A<string>(), Display.Url, r => r.DataProcessingRegistration_Id = registration.Id);

            //Act - check its possible to set a reference as master in a data processing registration
            using var setMasterResponse = await DataProcessingRegistrationHelper.SendSetMasterReferenceRequestAsync(registration.Id, reference.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.OK, setMasterResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_Set_Master_Reference_With_Invalid_Reference()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());

            //Act - check its not possible to set a reference as master in a data processing registration with a invalid reference id
            using var setMasterResponse = await DataProcessingRegistrationHelper.SendSetMasterReferenceRequestAsync(registration.Id, A<int>());

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, setMasterResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Available_Systems()
        {
            //Arrange
            var systemPrefix = A<Guid>().ToString("N");
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var system1Name = $"{systemPrefix}{1}";
            var system2Name = $"{systemPrefix}{2}";
            var filteredOutSystemName = A<string>();
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(system1Name, organizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(system2Name, organizationId, AccessModifier.Public);
            var filteredOutSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(filteredOutSystemName, organizationId, AccessModifier.Public);
            var usage1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organizationId);
            var usage2 = await ItSystemHelper.TakeIntoUseAsync(system2.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(filteredOutSystem.Id, organizationId);

            //Act
            var dtos = (await DataProcessingRegistrationHelper.GetAvailableSystemsAsync(registration.Id, systemPrefix)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            dtos.Select(x => new { x.Id, x.Name }).ToExpectedObject().ShouldMatch(new[] {new {usage1.Id, system1.Name}, new {usage2.Id, system2.Name}});
        }

        [Fact]
        public async Task Can_Assign_And_Remove_System()
        {
            //Arrange
            var systemPrefix = A<Guid>().ToString("N");
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var system1Name = $"{systemPrefix}{1}";
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(system1Name, organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Act - Add
            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(registration.Id, usage.Id);
            using var duplicateResponse = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(registration.Id, usage.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            var systemDTO = Assert.Single(dto.ItSystems);
            Assert.Equal(usage.Id, systemDTO.Id);
            Assert.Equal(system.Name, systemDTO.Name);

            //Act - remove
            using var removeResponse = await DataProcessingRegistrationHelper.SendRemoveSystemRequestAsync(registration.Id, usage.Id);
            using var duplicateRemoveResponse = await DataProcessingRegistrationHelper.SendRemoveSystemRequestAsync(registration.Id, usage.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, duplicateRemoveResponse.StatusCode);
            dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            Assert.Empty(dto.ItSystems);
        }

        [Fact]
        public async Task Can_Change_Oversight_Option()
        {
            //Arrange
            var name = A<string>();
            var dprDTO = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name);
            var oversightInterval = A<YearMonthIntervalOption>();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendChangeOversightIntervalOptionRequestAsync(dprDTO.Id, oversightInterval);

            //Assert
            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(dprDTO.Id);
            Assert.Equal(dto.OversightInterval.Value.Value, oversightInterval);
        }

        [Fact]
        public async Task Can_Change_Oversight_Option_Remark()
        {
            //Arrange
            var name = A<string>();
            var dprDTO = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name);
            var oversightRemark = A<string>();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendChangeOversightIntervalOptionRemarkRequestAsync(dprDTO.Id, oversightRemark);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(dprDTO.Id);
            Assert.Equal(dto.OversightInterval.Remark, oversightRemark);
        }

        [Fact]
        public async Task Can_Get_Available_DataProcessors()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var orgPrefix = A<string>();
            var orgName = $"{orgPrefix}_{A<int>()}";
            var organization = await OrganizationHelper.CreateOrganizationAsync(organizationId, orgName, "87654321", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());

            //Act
            var processors = await DataProcessingRegistrationHelper.GetAvailableDataProcessors(registration.Id, orgPrefix);

            //Assert
            Assert.Contains(processors, x => x.Id == organization.Id);
        }

        [Fact]
        public async Task Can_Assign_DataProcessors()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var organization = await OrganizationHelper.CreateOrganizationAsync(organizationId, A<string>(), "87654321", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());

            //Act
            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(registration.Id, organization.Id);
            using var duplicateResponse = await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(registration.Id, organization.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            var processor = Assert.Single(dto.DataProcessors);
            Assert.Equal(organization.Id, processor.Id);
            Assert.Equal(organization.Name, processor.Name);
            Assert.Equal(organization.Cvr, processor.CvrNumber);
        }

        [Fact]
        public async Task Can_Remove_DataProcessors()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var organization = await OrganizationHelper.CreateOrganizationAsync(organizationId, A<string>(), "87654321", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(registration.Id, organization.Id);

            //Act
            using var removeResponse = await DataProcessingRegistrationHelper.SendRemoveDataProcessorRequestAsync(registration.Id, organization.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            Assert.Empty(dto.DataProcessors);
        }

        [Fact]
        public async Task Can_Get_Available_SubDataProcessors()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var orgPrefix = A<string>();
            var orgName = $"{orgPrefix}_{A<int>()}";
            var organization = await OrganizationHelper.CreateOrganizationAsync(organizationId, orgName, "87654321", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());

            //Act
            var processors = await DataProcessingRegistrationHelper.GetAvailableSubDataProcessors(registration.Id, orgPrefix);

            //Assert
            Assert.Contains(processors, x => x.Id == organization.Id);
        }

        [Fact]
        public async Task Can_Assign_SubDataProcessors()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var organization = await OrganizationHelper.CreateOrganizationAsync(organizationId, A<string>(), "87654321", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
            using var setStateRequest = await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(registration.Id, YesNoUndecidedOption.Yes);
            Assert.Equal(HttpStatusCode.OK, setStateRequest.StatusCode);

            //Act
            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(registration.Id, organization.Id);
            using var duplicateResponse = await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(registration.Id, organization.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            var processor = Assert.Single(dto.SubDataProcessors);
            Assert.Equal(organization.Id, processor.Id);
            Assert.Equal(organization.Name, processor.Name);
            Assert.Equal(organization.Cvr, processor.CvrNumber);
        }

        [Fact]
        public async Task Can_Remove_SubDataProcessors()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var organization = await OrganizationHelper.CreateOrganizationAsync(organizationId, A<string>(), "87654321", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var registration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
            using var setStateRequest = await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(registration.Id, YesNoUndecidedOption.Yes);
            Assert.Equal(HttpStatusCode.OK, setStateRequest.StatusCode);
            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(registration.Id, organization.Id);

            //Act
            using var removeResponse = await DataProcessingRegistrationHelper.SendRemoveSubDataProcessorRequestAsync(registration.Id, organization.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            Assert.Empty(dto.SubDataProcessors);
        }

        [Fact]
        public async Task Can_Change_IsAgreementConcluded()
        {
            //Arrange
            var name = A<string>();
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var yesNoIrrelevantOption = A<YesNoIrrelevantOption>();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(registrationDto.Id, yesNoIrrelevantOption);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Change_AgreementConcludedAt()
        {
            //Arrange
            var name = A<string>();
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var dateTime = A<DateTime>();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendChangeAgreementConcludedAtRequestAsync(registrationDto.Id, dateTime);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Change_AgreementConcludedAt_To_Null()
        {
            //Arrange
            var name = A<string>();
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Act
            using var response = await DataProcessingRegistrationHelper.SendChangeAgreementConcludedAtRequestAsync(registrationDto.Id, null);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Assign_And_Remove_InsecureThirdCountry()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var countries = (await DataProcessingRegistrationHelper.GetCountryOptionsAsync(TestEnvironment.DefaultOrganizationId)).ToList();
            var randomCountry = countries[Math.Abs(A<int>()) % countries.Count];
            using var setStateRequest = await DataProcessingRegistrationHelper.SendSetUseTransferToInsecureThirdCountriesStateRequestAsync(registration.Id, YesNoUndecidedOption.Yes);
            Assert.Equal(HttpStatusCode.OK, setStateRequest.StatusCode);

            //Act - add country
            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignInsecureThirdCountryRequestAsync(registration.Id, randomCountry.Id);
            using var duplicateResponse = await DataProcessingRegistrationHelper.SendAssignInsecureThirdCountryRequestAsync(registration.Id, randomCountry.Id);

            //Assert - country added
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            Assert.Equal(YesNoUndecidedOption.Yes, dto.TransferToInsecureThirdCountries);
            var country = Assert.Single(dto.InsecureThirdCountries);
            Assert.Equal(randomCountry.Id, country.Id);
            Assert.Equal(randomCountry.Name, country.Name);

            //Act - remove
            using var removeResponse = await DataProcessingRegistrationHelper.SendRemoveInsecureThirdCountryRequestAsync(registration.Id, randomCountry.Id);

            //Assert country removed again
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            Assert.Empty(dto.InsecureThirdCountries);
        }

        [Fact]
        public async Task Can_Assign_And_Clear_BasisForTransfer()
        {
            //Arrange
            var registration = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var options = (await DataProcessingRegistrationHelper.GetBasisForTransferOptionsAsync(TestEnvironment.DefaultOrganizationId)).ToList();
            var randomOption = options[Math.Abs(A<int>()) % options.Count];

            //Act - assign basis for transfer
            using var assignResponse = await DataProcessingRegistrationHelper.SendAssignBasisForTransferRequestAsync(registration.Id, randomOption.Id);

            //Assert - basis for transfer set
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
            var dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);

            Assert.NotNull(dto.BasisForTransfer);
            Assert.Equal(randomOption.Id, dto.BasisForTransfer.Id);
            Assert.Equal(randomOption.Name, dto.BasisForTransfer.Name);

            //Act - remove
            using var removeResponse = await DataProcessingRegistrationHelper.SendClearBasisForTransferRequestAsync(registration.Id);

            //Assert country removed again
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            dto = await DataProcessingRegistrationHelper.GetAsync(registration.Id);
            Assert.Null(dto.BasisForTransfer);
        }

        [Fact]
        public async Task Can_Get_AvailableDataResponsibleOptions()
        {
            //Arrange
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>()).ConfigureAwait(false);

            //Act
            var dataProcessingOptions = await DataProcessingRegistrationHelper.GetAvailableDataResponsibleOptionsRequestAsync(registrationDto.Id);

            //Assert
            Assert.NotEmpty(dataProcessingOptions.DataResponsibleOptions);
        }

        [Fact]
        public async Task Can_Change_DataResponsible()
        {
            //Arrange
            var name = A<string>();
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var dataOptions = await DataProcessingRegistrationHelper.GetAvailableDataResponsibleOptionsRequestAsync(registrationDto.Id);
            var dataResponsibleOption = dataOptions.DataResponsibleOptions.First();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendAssignDataResponsibleRequestAsync(registrationDto.Id, dataResponsibleOption.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updateRegistrationDto = await DataProcessingRegistrationHelper.GetAsync(registrationDto.Id);
            Assert.Equal(dataResponsibleOption.Id, updateRegistrationDto.DataResponsible.Value.Id);
        }

        [Fact]
        public async Task Can_Change_DataResponsible_ToNull()
        {
            //Arrange
            var name = A<string>();
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var dataOptions = await DataProcessingRegistrationHelper.GetAvailableDataResponsibleOptionsRequestAsync(registrationDto.Id);
            var dataResponsibleOption = dataOptions.DataResponsibleOptions.First();
            using var assingResponse = await DataProcessingRegistrationHelper.SendAssignDataResponsibleRequestAsync(registrationDto.Id, dataResponsibleOption.Id);
            Assert.Equal(HttpStatusCode.OK, assingResponse.StatusCode);

            //Act
            using var clearResponse = await DataProcessingRegistrationHelper.SendClearDataResponsibleRequestAsync(registrationDto.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, clearResponse.StatusCode);
            var updateRegistrationDto = await DataProcessingRegistrationHelper.GetAsync(registrationDto.Id);
            Assert.Null(updateRegistrationDto.DataResponsible.Value);
        }

        [Fact]
        public async Task Can_Change_DataResponsibleRemark()
        {
            //Arrange
            var name = A<string>();
            var registrationDto = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var remark = A<string>();

            //Act
            using var response = await DataProcessingRegistrationHelper.SendUpdateDataResponsibleRemarkRequestAsync(registrationDto.Id, remark);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updateRegistrationDto = await DataProcessingRegistrationHelper.GetAsync(registrationDto.Id);
            Assert.Equal(remark, updateRegistrationDto.DataResponsible.Remark);
        }
    }
}
