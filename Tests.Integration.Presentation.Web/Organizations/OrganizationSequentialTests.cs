using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    [Collection(nameof(SequentialTestGroup))]
    public class OrganizationSequentialTests : WithAutoFixture
    {
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
        public async Task Get_GET_DeletionConflicts_And_DELETE_Organization_With_Enforce_Set_To_True()
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

            // System exposing interface in another organization (also expose one which is not conflicting and check that it is not in the conflicts list)
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
            await ItSystemHelper.SendSetParentSystemRequestAsync(systemInAnotherOrg2.Id, systemSetAsParentToSystemsInOtherOrgs.Id, anotherOrg1.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            // Conflicts due to DPR where org is set as data processor or sub data processor (the two dprs in own org should not be included in the final result)
            var dprOwnOrg1 = await DataProcessingRegistrationHelper.CreateAsync(organization.Id, A<string>());
            var dprOwnOrg2 = await DataProcessingRegistrationHelper.CreateAsync(organization.Id, A<string>());
            await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(dprOwnOrg1.Id, organization.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(dprOwnOrg2.Id, YesNoUndecidedOption.Yes).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(dprOwnOrg2.Id, organization.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            var dprConflictOnDataProcessor = await DataProcessingRegistrationHelper.CreateAsync(anotherOrg1.Id, A<string>());
            var dprConflictOnSubDataProcessor = await DataProcessingRegistrationHelper.CreateAsync(anotherOrg1.Id, A<string>());
            await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(dprConflictOnDataProcessor.Id, organization.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(dprConflictOnSubDataProcessor.Id, YesNoUndecidedOption.Yes).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(dprConflictOnSubDataProcessor.Id, organization.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Contracts in other orgs where deleted org is supplier
            var contractInOwnOrgNoConflict = await ItContractHelper.CreateContract(A<string>(), organization.Id);
            var contractAnotherOrgOrgWithConflict = await ItContractHelper.CreateContract(A<string>(), anotherOrg1.Id);

            await ItContractHelper.SendAssignSupplierAsync(contractInOwnOrgNoConflict.Id, organization.Id, organization.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItContractHelper.SendAssignSupplierAsync(contractAnotherOrgOrgWithConflict.Id, organization.Id, anotherOrg1.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Systems in other orgs where org is rightsholder
            var systemInOwnOrgNoRightsHolderConflict = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organization.Id, AccessModifier.Public);
            var systemInAnotherOrgWithRightsHolderConflict = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), anotherOrg1.Id, AccessModifier.Public);
            await ItSystemHelper.SendSetBelongsToRequestAsync(systemInOwnOrgNoRightsHolderConflict.Id, organization.Id, organization.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItSystemHelper.SendSetBelongsToRequestAsync(systemInAnotherOrgWithRightsHolderConflict.Id, organization.Id, anotherOrg1.Id).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

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

            //DPR conflicts on data processor and sub data processor
            var dataProcessorConflict = Assert.Single(conflicts.DprInOtherOrganizationsWhereOrgIsDataProcessor);
            var subDataProcessorConflict = Assert.Single(conflicts.DprInOtherOrganizationsWhereOrgIsSubDataProcessor);
            AssertNamedEntityWithOrganizationalRelationship(dprConflictOnDataProcessor.Id, dprConflictOnDataProcessor.Name, anotherOrg1.Id, anotherOrg1.Name, dataProcessorConflict);
            AssertNamedEntityWithOrganizationalRelationship(dprConflictOnSubDataProcessor.Id, dprConflictOnSubDataProcessor.Name, anotherOrg1.Id, anotherOrg1.Name, subDataProcessorConflict);

            // Contract in another org with deleted org set as suppllier
            var contractConflict = Assert.Single(conflicts.ContractsInOtherOrganizationsWhereOrgIsSupplier);
            AssertNamedEntityWithOrganizationalRelationship(contractAnotherOrgOrgWithConflict.Id, contractAnotherOrgOrgWithConflict.Name, anotherOrg1.Id, anotherOrg1.Name, contractConflict);

            //Systems in other orgs with deleted org as rights holder
            var rightsHolderConflict = Assert.Single(conflicts.SystemsInOtherOrganizationsWhereOrgIsRightsHolder);
            AssertNamedEntityWithOrganizationalRelationship(systemInAnotherOrgWithRightsHolderConflict.Id, systemInAnotherOrgWithRightsHolderConflict.Name, anotherOrg1.Id, anotherOrg1.Name, rightsHolderConflict);

            // ACT - DELETE The organization
            await OrganizationHelper.SendDeleteOrganizationRequestAsync(organization.Uuid, true).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            // Assert
            // Check 0 - organization is indeed gone
            await OrganizationHelper.SendGetOrganizationRequestAsync(organization.Id).WithExpectedResponseCode(HttpStatusCode.NotFound).DisposeAsync();

            // Check 1: Verify that systems we expect to be moved to default org have been moved (they are used by / exposing interfaces from other orgs)
            systemUsedInAnotherOrg = await ItSystemHelper.GetSystemAsync(systemUsedInAnotherOrg.Id);
            Assert.Equal(TestEnvironment.DefaultOrganizationId, systemUsedInAnotherOrg.OrganizationId);

            systemSetAsParentToSystemsInOtherOrgs = await ItSystemHelper.GetSystemAsync(systemSetAsParentToSystemsInOtherOrgs.Id);
            Assert.Equal(TestEnvironment.DefaultOrganizationId, systemSetAsParentToSystemsInOtherOrgs.OrganizationId);

            systemExposingInterfaceInAnotherOrg = await ItSystemHelper.GetSystemAsync(systemExposingInterfaceInAnotherOrg.Id);
            Assert.Equal(TestEnvironment.DefaultOrganizationId, systemExposingInterfaceInAnotherOrg.OrganizationId);

            // Check 2: Verify that interfaces we expect to be moved to default org have been moved (they are exposed on systems from other orgs)
            interfaceExposedOnSystemInAnotherOrg = await InterfaceHelper.GetInterfaceById(interfaceExposedOnSystemInAnotherOrg.Id);
            Assert.Equal(TestEnvironment.DefaultOrganizationId, interfaceExposedOnSystemInAnotherOrg.OrganizationId);

            // Check 3: Verify that rights holder has been cleared on systems which had the old org as rights holder
            systemInAnotherOrgWithRightsHolderConflict = await ItSystemHelper.GetSystemAsync(systemInAnotherOrgWithRightsHolderConflict.Id);
            Assert.Null(systemInAnotherOrgWithRightsHolderConflict.BelongsToId);

            // Check 4: Verify that supplier is cleared on contracts that had the org as supplier
            contractAnotherOrgOrgWithConflict = await ItContractHelper.GetItContract(contractAnotherOrgOrgWithConflict.Id);
            Assert.Null(contractAnotherOrgOrgWithConflict.SupplierId);

            // Check 5: Verify that DPR conflicts have been resolved by removing org from data processor and sub data processor
            dprConflictOnDataProcessor = await DataProcessingRegistrationHelper.GetAsync(dprConflictOnDataProcessor.Id);
            Assert.DoesNotContain(dprConflictOnDataProcessor.DataProcessors, x => x.Id == organization.Id);

            dprConflictOnSubDataProcessor = await DataProcessingRegistrationHelper.GetAsync(dprConflictOnSubDataProcessor.Id);
            Assert.DoesNotContain(dprConflictOnSubDataProcessor.SubDataProcessors, x => x.Id == organization.Id);
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
    }
}
