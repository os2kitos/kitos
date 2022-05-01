using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    public class OrganizationTest : WithAutoFixture
    {
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Set_ContactPerson(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var contactPersonDto = await OrganizationHelper.GetContactPersonAsync(organizationId);
            var name = A<string>();
            var lastName = A<string>();
            var email = A<string>();
            var phone = A<string>();

            //Act - perform the action with the actual role
            var result = await OrganizationHelper.ChangeContactPersonAsync(contactPersonDto.Id, organizationId, name, lastName, email, phone, login);

            //Assert
            Assert.Equal(contactPersonDto.Id, result.Id);
            Assert.Equal(email, result.Email);
            Assert.Equal(name, result.Name);
            Assert.Equal(lastName, result.LastName);
            Assert.Equal(phone, result.PhoneNumber);
            Assert.Equal(organizationId, result.OrganizationId);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Set_ContactPerson(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var contactPersonDto = await OrganizationHelper.GetContactPersonAsync(organizationId);
            var name = A<string>();
            var lastName = A<string>();
            var email = A<string>();
            var phone = A<string>();

            //Act - perform the action with the actual role
            using var result = await OrganizationHelper.SendChangeContactPersonRequestAsync(contactPersonDto.Id, organizationId, name, lastName, email, phone, login);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.AndenOffentligMyndighed)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.Interessefællesskab)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.Kommune)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.Virksomhed)]
        public async Task Can_Create_Organization_Of_Type(OrganizationRole role, OrganizationTypeKeys organizationType)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var name = A<string>();
            var cvr = (A<int>() % 9999999999).ToString("D10");
            const AccessModifier accessModifier = AccessModifier.Public;

            //Act - perform the action with the actual role
            var result = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, name, cvr, organizationType, accessModifier, login);

            //Assert
            Assert.Equal(accessModifier, result.AccessModifier);
            Assert.Equal(name, (string)result.Name);
            Assert.Equal(cvr, (string)result.Cvr);
        }

        [Theory]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.AndenOffentligMyndighed)]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.Interessefællesskab)]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.Kommune)]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.Virksomhed)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.Kommune)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.AndenOffentligMyndighed)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.Interessefællesskab)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.Virksomhed)]
        public async Task Cannot_Create_Organization_Of_Type(OrganizationRole role, OrganizationTypeKeys organizationType)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var name = A<string>();
            var cvr = (A<int>() % 9999999999).ToString("D10");
            const AccessModifier accessModifier = AccessModifier.Public;

            //Act - perform the action with the actual role
            using var result = await OrganizationHelper.SendCreateOrganizationRequestAsync(TestEnvironment.DefaultOrganizationId, name, cvr, organizationType, accessModifier, login);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Organizations_Filtered_By_Cvr_Or_Name()
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var nameOrg1 = A<string>();
            var cvrOrg1 = (A<int>() % 9999999999).ToString("D10");
            const AccessModifier accessModifier = AccessModifier.Public;

            //Act - perform the action with the actual role
            using var _ = await OrganizationHelper.SendCreateOrganizationRequestAsync(TestEnvironment.DefaultOrganizationId, nameOrg1, cvrOrg1, OrganizationTypeKeys.Kommune, accessModifier, login);

            using var organizationsFilteredByCvr = await OrganizationHelper.SendGetOrganizationSearchRequestAsync(cvrOrg1);
            Assert.True(organizationsFilteredByCvr.IsSuccessStatusCode);

            var resultFilteredByCvr = await organizationsFilteredByCvr.ReadResponseBodyAsKitosApiResponseAsync<List<Organization>>();
            Assert.True(resultFilteredByCvr.Exists(prp => prp.Cvr.Contains(cvrOrg1)));

            using var organizationsFilteredByName = await OrganizationHelper.SendGetOrganizationSearchRequestAsync(nameOrg1);
            Assert.True(organizationsFilteredByName.IsSuccessStatusCode);

            var resultFilteredByName = await organizationsFilteredByName.ReadResponseBodyAsKitosApiResponseAsync<List<Organization>>();
            Assert.True(resultFilteredByName.Exists(prp => prp.Name.Contains(nameOrg1)));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        public async Task Can_Delete_Organization_Without_Conflicts(OrganizationRole role, bool expectAllowedToDelete)
        {
            //Arrange
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var loginCookie = await HttpApi.GetCookieAsync(role);

            //Act
            using var response = await OrganizationHelper.SendDeleteOrganizationRequestAsync(organization.Uuid, false, loginCookie);

            //Assert
            Assert.Equal(expectAllowedToDelete ? HttpStatusCode.OK : HttpStatusCode.Forbidden, response.StatusCode);
            await AssertGetOrganizationResponse(organization.Uuid, expectAllowedToDelete ? HttpStatusCode.NotFound : HttpStatusCode.OK);

        }

        [Fact]
        public async Task Cannot_Delete_If_Deletion_Has_Conflicts_And_Enforce_Is_False()
        {
            //Arrange
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var systemUsedInAnotherOrg = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organization.Id, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(systemUsedInAnotherOrg.Id, TestEnvironment.DefaultOrganizationId);

            //Act
            using var response = await OrganizationHelper.SendDeleteOrganizationRequestAsync(organization.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            await AssertGetOrganizationResponse(organization.Uuid, HttpStatusCode.OK);

        }

        [Fact]
        public async Task Can_Delete_If_Deletion_Has_Conflicts_And_Enforce_Is_True()
        {
            //Arrange
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var systemUsedInAnotherOrg = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organization.Id, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(systemUsedInAnotherOrg.Id, TestEnvironment.DefaultOrganizationId);

            //Act
            using var response = await OrganizationHelper.SendDeleteOrganizationRequestAsync(organization.Uuid, true);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await AssertGetOrganizationResponse(organization.Uuid, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_GET_DeletionConflicts_And_Delete_With_Enforce_Set_To_True()
        {
            //Arrange
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var anotherOrg1 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);

            //System used in another organization
            var systemUsedInAnotherOrg = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organization.Id, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(systemUsedInAnotherOrg.Id, anotherOrg1.Id);

            //Interface exposed on system in another org
            var systemInAnotherOrg1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), anotherOrg1.Id, AccessModifier.Public);
            var interfaceExposedOnSystemInAnotherOrg = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), null, organization.Id, AccessModifier.Public));
            await InterfaceExhibitHelper.CreateExhibit(systemInAnotherOrg1.Id, interfaceExposedOnSystemInAnotherOrg.Id);

            // System exposing interface in another organization (also exopose one which is not conflicting and check that it is not in the conflicts list)
            var interfaceInOwnOrgOrg1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), null, organization.Id, AccessModifier.Public));
            var interfaceInAnotherOrg1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), null, anotherOrg1.Id, AccessModifier.Public));
            var systemExposingInterfaceInAnotherOrg = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organization.Id, AccessModifier.Public);
            await InterfaceExhibitHelper.CreateExhibit(systemExposingInterfaceInAnotherOrg.Id, interfaceInAnotherOrg1.Id);
            await InterfaceExhibitHelper.CreateExhibit(systemExposingInterfaceInAnotherOrg.Id, interfaceInOwnOrgOrg1.Id); //should not show up in conflicts

            // Systems set as parents to systems in other orgs (expect one result because one of the children are in the same org)
            var systemInOwnOrg2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organization.Id, AccessModifier.Public);
            var systemInAnotherOrg2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), anotherOrg1.Id, AccessModifier.Public);
            var systemSetAsParentToSystemsInOtherOrgs = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organization.Id, AccessModifier.Public);
            await ItSystemHelper.SendSetParentSystemRequestAsync(systemInOwnOrg2.Id, systemSetAsParentToSystemsInOtherOrgs.Id, organization.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItSystemHelper.SendSetParentSystemRequestAsync(systemInAnotherOrg2.Id, systemSetAsParentToSystemsInOtherOrgs.Id,anotherOrg1.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();


            //TODO: DataProcessorForDprInOtherOrg
            //TODO: SubDataProcessorForDprInOtherOrg
            //TODO: ContractsInOtherOrgWhereOrgIsSupplier
            //TODO: SystemsInOtherOrgWhereOrgIsRightsHolder

            //Act
            var conflicts = await OrganizationHelper.GetOrganizationRemovalConflictsAsync(organization.Uuid);

            //Assert - systems used by other organizations
            var usageOutsideOrganization = Assert.Single(conflicts.SystemsWithUsagesOutsideTheOrganization);
            AssertNamedEntity(systemUsedInAnotherOrg.Id, systemUsedInAnotherOrg.Name, usageOutsideOrganization.System);
            var org = Assert.Single(usageOutsideOrganization.OtherOrganizationsWhichUseTheSystem);
            AssertNamedEntity(anotherOrg1.Id, anotherOrg1.Name, org);

            //Interfaces exposed by systems in other organizations
            var interfaceExposedBySystemsInOtherOrgs = Assert.Single(conflicts.InterfacesExposedOnSystemsOutsideTheOrganization);
            AssertNamedEntity(interfaceExposedOnSystemInAnotherOrg.Id, interfaceExposedOnSystemInAnotherOrg.Name, interfaceExposedBySystemsInOtherOrgs.ExposedInterface);
            AssertNamedEntityWithOrganizationalRelationship(systemInAnotherOrg1.Id, systemInAnotherOrg1.Name, anotherOrg1.Id, anotherOrg1.Name, interfaceExposedBySystemsInOtherOrgs.ExposedBy);

            // System exposing interface in another organization
            var systemExposingInterfaceInOtherOrgs = Assert.Single(conflicts.SystemsExposingInterfacesDefinedInOtherOrganizations);
            AssertNamedEntity(systemExposingInterfaceInAnotherOrg.Id, systemExposingInterfaceInAnotherOrg.Name, systemExposingInterfaceInOtherOrgs.System);
            var conflictInterface = Assert.Single(systemExposingInterfaceInOtherOrgs.ExposedInterfaces);
            AssertNamedEntityWithOrganizationalRelationship(interfaceInAnotherOrg1.Id, interfaceInAnotherOrg1.Name, anotherOrg1.Id, anotherOrg1.Name, conflictInterface);

            // Systems set as parents to systems in other orgs (expect one result because one of the children are in the same org)
            var systemWithChildrenInOtherOrgs = Assert.Single(conflicts.SystemsSetAsParentSystemToSystemsInOtherOrganizations);
            AssertNamedEntity(systemSetAsParentToSystemsInOtherOrgs.Id, systemSetAsParentToSystemsInOtherOrgs.Name, systemWithChildrenInOtherOrgs.System);
            var conflictingChild = Assert.Single(systemWithChildrenInOtherOrgs.Children);
            AssertNamedEntityWithOrganizationalRelationship(systemInAnotherOrg2.Id, systemInAnotherOrg2.Name, anotherOrg1.Id, anotherOrg1.Name, conflictingChild);
        }

        private static void AssertNamedEntity(int expectedId, string expectedName, NamedEntityDTO dto)
        {
            Assert.Equal(expectedId, dto.Id);
            Assert.Equal(expectedName, dto.Name);
        }

        private static void AssertNamedEntityWithOrganizationalRelationship(int expectedId, string expectedName, int expectedOrgId, string expectedOrgName, EntityWithOrganizationRelationshipDTO dto)
        {
            Assert.Equal(expectedId, dto.Id);
            Assert.Equal(expectedName, dto.Name);
            Assert.Equal(expectedOrgId, dto.Organization.Id);
            Assert.Equal(expectedOrgName, dto.Organization.Name);
        }


        private static async Task AssertGetOrganizationResponse(Guid organizationId, HttpStatusCode expected)
        {
            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            using var getAfterDeleteResponse = await OrganizationV2Helper.SendGetOrganizationAsync(globalAdminToken.Token, organizationId);
            Assert.Equal(expected, getAfterDeleteResponse.StatusCode);
        }

        //TODO: Create a full conflicts helper.. reuse that in the full deletion test
        //TODO: Get data in all conflicts
        //TODO: Delete with full conflicts
    }
}
