using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.SystemRelations;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Response.System;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.SystemUsage
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItSystemUsageOverviewReadModelsTest : WithAutoFixture
    {
        private const string TestCvr = "11224455";

        [Fact]
        public async Task Can_Query_And_Page_ReadModels_Using_Db_Id()
        {
            await TestQueryAndPaging(async q => await ItSystemUsageHelper.QueryReadModelByNameContent(q.orgId, q.query, q.top, q.skip));
        }

        [Fact]
        public async Task Can_Query_And_Page_ReadModels_Using_Uuid()
        {
            await TestQueryAndPaging(async q =>
            {
                var orgUuid = DatabaseAccess.GetEntityUuid<Organization>(q.orgId);
                return await ItSystemUsageHelper.QueryReadModelByNameContent(orgUuid, q.query, q.top, q.skip);
            });
        }

        private async Task TestQueryAndPaging(Func<(int orgId, string query, int top, int skip), Task<IEnumerable<ItSystemUsageOverviewReadModel>>> getPage)
        {
            //Arrange
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var suffix = A<Guid>().ToString("N");
            var name1 = $"1_{suffix}";
            var name2 = $"2_{suffix}";
            var name3 = $"3_{suffix}";

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name1, organizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name2, organizationId, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name3, organizationId, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system1.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system2.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system3.Id, organizationId);


            //Act
            var page1 = (await getPage((organizationId, suffix, 2, 0))).ToList();
            var page2 = (await getPage((organizationId, suffix, 2, 2))).ToList();

            //Assert
            Assert.Equal(2, page1.Count);
            Assert.Equal(name1, page1.First().SystemName);
            Assert.Equal(name2, page1.Last().SystemName);

            Assert.Single(page2);
            Assert.Equal(name3, page2.Single().SystemName);
        }

        [Fact]
        public async Task ReadModels_Contain_Correct_Content()
        {
            //Arrange
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var organizationName = TestEnvironment.DefaultOrganizationName;

            var systemName = A<string>();
            var systemPreviousName = A<string>();
            var systemDescription = A<string>();
            var systemDisabled = A<bool>();

            var systemParentName = A<string>();
            var systemParentDisabled = A<bool>();

            var systemUsageVersion = A<string>();
            var systemUsageLocalCallName = A<string>();
            var systemUsageLocalSystemId = A<string>();
            var concluded = DateTime.UtcNow.AddDays(-A<int>());
            var systemUsageExpirationDate = DateTime.UtcNow.AddDays(A<int>());
            var archiveDuty = A<ArchiveDutyTypes>();
            var riskAssessment = A<DataOptions>();
            var riskAssessmentDate = A<DateTime>();
            var linkToDirectoryUrl = A<string>();
            var linkToDirectoryUrlName = A<string>();
            var riskSupervisionDocumentationUrl = A<string>();
            var riskSupervisionDocumentationUrlName = A<string>();
            var generalPurpose = A<string>();
            var hostedAt = A<HostedAt>();
            var userCount = A<UserCount>();

            var contract1Name = A<string>();
            var contract2Name = A<string>();

            var dataProcessingRegistrationName = A<string>();

            var system = await PrepareItSystem(systemName, systemPreviousName, systemDescription, organizationId, organizationName, AccessModifier.Public);
            var systemParent = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemParentName, organizationId, AccessModifier.Public);
            var systemParentUsage = await ItSystemHelper.TakeIntoUseAsync(systemParent.Id, organizationId);

            var systemId = DatabaseAccess.GetEntityId<Core.DomainModel.ItSystem.ItSystem>(system.Uuid);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(systemId, organizationId);

            // Role assignment
            var businessRoleDtos = await ItSystemUsageHelper.GetAvailableRolesAsync(organizationId);
            var role = businessRoleDtos.First();
            var availableUsers = await ItSystemUsageHelper.GetAvailableUsersAsync(organizationId);
            var user = availableUsers.First();
            using var assignRoleResponse = await ItSystemUsageHelper.SendAssignRoleRequestAsync(systemUsage.Id, organizationId, role.Id, user.Id);
            Assert.Equal(HttpStatusCode.Created, assignRoleResponse.StatusCode);

            // System changes
            await ItSystemHelper.SendSetDisabledRequestAsync(systemId, systemDisabled).WithExpectedResponseCode(HttpStatusCode.NoContent).DisposeAsync();
            await ItSystemHelper.SendSetParentSystemRequestAsync(systemId, systemParent.Id, organizationId).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItSystemHelper.SendSetBelongsToRequestAsync(systemId, organizationId, organizationId).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync(); // Using default organization as BelongsTo

            var availableBusinessTypeOptions = (await ItSystemHelper.GetBusinessTypeOptionsAsync(organizationId)).ToList();
            var businessType = availableBusinessTypeOptions[Math.Abs(A<int>()) % availableBusinessTypeOptions.Count];
            await ItSystemHelper.SendSetBusinessTypeRequestAsync(systemId, businessType.Id, organizationId).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var taskRefs = (await ItSystemHelper.GetAvailableTaskRefsRequestAsync(systemId)).ToList();
            var taskRef = taskRefs[Math.Abs(A<int>()) % taskRefs.Count];
            await ItSystemHelper.SendAddTaskRefRequestAsync(systemId, taskRef.TaskRef.Id, organizationId).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            // Parent system 
            await ItSystemHelper.SendSetDisabledRequestAsync(systemParent.Id, systemParentDisabled).WithExpectedResponseCode(HttpStatusCode.NoContent).DisposeAsync();

            var dataClassification =
                (await EntityOptionHelper.GetOptionsAsync(EntityOptionHelper.ResourceNames.ItSystemCategories,
                    organizationId)).RandomItem();

            // System Usage changes
            var body = new
            {
                ExpirationDate = systemUsageExpirationDate,
                Version = systemUsageVersion,
                LocalCallName = systemUsageLocalCallName,
                LocalSystemId = systemUsageLocalSystemId,
                Concluded = concluded,
                ArchiveDuty = archiveDuty,
                RiskAssessment = riskAssessment,
                RiskAssessmentDate = riskAssessmentDate,
                linkToDirectoryUrl,
                linkToDirectoryUrlName,
                riskSupervisionDocumentationUrl,
                riskSupervisionDocumentationUrlName,
                GeneralPurpose = generalPurpose,
                UserCount = userCount,
                ItSystemCategoriesId = dataClassification.Id
            };
            await ItSystemUsageHelper.PatchSystemUsage(systemUsage.Id, organizationId, body);
            var sensitiveDataLevel = await ItSystemUsageHelper.AddSensitiveDataLevel(systemUsage.Id, A<SensitiveDataLevel>());
            var isHoldingDocument = A<bool>();
            await ItSystemUsageHelper.SetIsHoldingDocumentRequestAsync(systemUsage.Id, isHoldingDocument);
            
            // Responsible Organization Unit and relevant units
            var orgUnitName1 = A<string>();
            var orgUnitName2 = A<string>();
            var organizationUnit1 = await OrganizationHelper.CreateOrganizationUnitAsync(organizationId, orgUnitName1);
            var organizationUnit2 = await OrganizationHelper.CreateOrganizationUnitAsync(organizationId, orgUnitName2);
            var responsibleUnit = new[] { organizationUnit1, organizationUnit2 }.RandomItem();
            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit1.Id, organizationId).DisposeAsync();
            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit2.Id, organizationId).DisposeAsync();
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, responsibleUnit.Id).DisposeAsync();

            //References
            var reference = await ReferencesHelper.CreateReferenceAsync(A<string>(), A<string>(), A<string>(), dto => dto.ItSystemUsage_Id = systemUsage.Id);

            //Main Contract
            var contract1 = await ItContractHelper.CreateContract(contract1Name, organizationId);
            var contract2 = await ItContractHelper.CreateContract(contract2Name, organizationId);
            var contractUpdateBody = new
            {
                supplierId = organizationId
            };
            await ItContractHelper.PatchContract(contract1.Id, organizationId, contractUpdateBody);
            await ItContractHelper.AddItSystemUsage(contract1.Id, systemUsage.Id, organizationId);
            await ItContractHelper.AddItSystemUsage(contract2.Id, systemUsage.Id, organizationId);

            await ItSystemUsageHelper.SendSetMainContractRequestAsync(systemUsage.Id, contract1.Id).DisposeAsync();

            // ArchivePeriods
            var archivePeriodStartDate = DateTime.Now.AddDays(-1);
            var archivePeriodEndDate = DateTime.Now.AddDays(1);
            await ItSystemUsageHelper.AddArchivePeriodAsync(systemUsage.Id, archivePeriodStartDate, archivePeriodEndDate, organizationId);


            // DataProcessingRegistrations
            var yesNoIrrelevantOption = A<YesNoIrrelevantOption>();
            var dataProcessingRegistration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, dataProcessingRegistrationName);
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dataProcessingRegistration.Id, yesNoIrrelevantOption).DisposeAsync();
            await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dataProcessingRegistration.Id, systemUsage.Id).DisposeAsync();

            // DependsOnInterfaces + IncomingSystemUsages + outgoing system usages
            var outgoingRelationSystemName = A<string>();
            var incomingRelationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var relationInterfaceId = A<string>();

            var incomingRelationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(incomingRelationSystemName, organizationId, AccessModifier.Public);
            var incomingRelationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(incomingRelationSystem.Id, organizationId);

            var outGoingRelationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(outgoingRelationSystemName, organizationId, AccessModifier.Public);
            var outgoingRelationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(outGoingRelationSystem.Id, organizationId);

            var relationInterfaceDTO = InterfaceHelper.CreateInterfaceDto(relationInterfaceName, relationInterfaceId, organizationId, AccessModifier.Public);
            var relationInterface = await InterfaceHelper.CreateInterface(relationInterfaceDTO);
            await InterfaceExhibitHelper.CreateExhibit(outGoingRelationSystem.Id, relationInterface.Id);

            var incomingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = incomingRelationSystemUsage.Id,
                ToUsageId = systemUsage.Id
            };
            await SystemRelationHelper.PostRelationAsync(incomingRelationDTO);

            var outgoingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = systemUsage.Id,
                ToUsageId = outgoingRelationSystemUsage.Id,
                InterfaceId = relationInterface.Id
            };
            await SystemRelationHelper.PostRelationAsync(outgoingRelationDTO);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Get current system usage
            var updatedSystemUsage = await ItSystemUsageHelper.GetItSystemUsageRequestAsync(systemUsage.Id);

            //Act 
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            // From System Usage
            Assert.Equal(systemUsage.Id, readModel.SourceEntityId);
            Assert.Equal(systemUsage.Uuid, readModel.SourceEntityUuid);
            Assert.Equal(organizationId, readModel.OrganizationId);
            Assert.Equal(systemUsageVersion, readModel.Version);
            Assert.Equal(systemUsageLocalCallName, readModel.LocalCallName);
            Assert.Equal(updatedSystemUsage.ObjectOwnerFullName, readModel.ObjectOwnerName);
            Assert.Equal(updatedSystemUsage.ObjectOwnerFullName, readModel.LastChangedByName); // Same user was used to create and change the systemUsage
            Assert.Equal(concluded.Date, readModel.Concluded?.Date);
            Assert.Equal(systemUsageExpirationDate.Date, readModel.ExpirationDate?.Date);
            Assert.True(readModel.ActiveAccordingToValidityPeriod);
            Assert.True(readModel.ActiveAccordingToLifeCycle);
            Assert.True(readModel.SystemActive);
            Assert.Equal(updatedSystemUsage.LastChanged.Date, readModel.LastChangedAt.Date);
            Assert.Equal(archiveDuty, readModel.ArchiveDuty);
            Assert.Equal(isHoldingDocument, readModel.IsHoldingDocument);
            Assert.Equal(linkToDirectoryUrlName, readModel.LinkToDirectoryName);
            Assert.Equal(linkToDirectoryUrl, readModel.LinkToDirectoryUrl);
            Assert.Equal(generalPurpose, readModel.GeneralPurpose);
            Assert.Equal(hostedAt, readModel.HostedAt);
            Assert.Equal(userCount, readModel.UserCount);

            if (riskAssessment == DataOptions.YES)
            {
                Assert.Equal(riskSupervisionDocumentationUrlName, readModel.RiskSupervisionDocumentationName);
                Assert.Equal(riskSupervisionDocumentationUrl, readModel.RiskSupervisionDocumentationUrl);
                Assert.Equal(riskAssessmentDate, readModel.RiskAssessmentDate);
            }
            else
            {
                Assert.Null(readModel.RiskSupervisionDocumentationName);
                Assert.Null(readModel.RiskSupervisionDocumentationUrl);
                Assert.Null(readModel.RiskAssessmentDate);
            }

            // Sensitive Data Level
            var rmSensitiveDataLevel = Assert.Single(readModel.SensitiveDataLevels);
            Assert.Equal(sensitiveDataLevel.DataSensitivityLevel, rmSensitiveDataLevel.SensitivityDataLevel);
            Assert.Equal(sensitiveDataLevel.DataSensitivityLevel.GetReadableName(), readModel.SensitiveDataLevelsAsCsv);

            // From System
            Assert.Equal(systemName, readModel.SystemName);
            Assert.Equal(systemPreviousName, readModel.SystemPreviousName);
            Assert.Equal(systemDescription, readModel.SystemDescription);
            Assert.Equal(systemDisabled, readModel.ItSystemDisabled);
            Assert.Equal(system.Uuid.ToString("D"), readModel.ItSystemUuid);
            Assert.Equal(businessType.Id, readModel.ItSystemBusinessTypeId);
            Assert.Equal(businessType.Uuid, readModel.ItSystemBusinessTypeUuid);
            Assert.Equal(businessType.Name, readModel.ItSystemBusinessTypeName);
            Assert.Equal(dataClassification.Uuid, readModel.ItSystemCategoriesUuid);
            Assert.Equal(dataClassification.Name, readModel.ItSystemCategoriesName);
            Assert.Equal(organizationId, readModel.ItSystemRightsHolderId);
            Assert.Equal(organizationName, readModel.ItSystemRightsHolderName);
            Assert.Equal(taskRef.TaskRef.TaskKey ?? string.Empty, readModel.ItSystemKLEIdsAsCsv);
            Assert.Equal(taskRef.TaskRef.Description, readModel.ItSystemKLENamesAsCsv);
            var readTaskRef = Assert.Single(readModel.ItSystemTaskRefs);
            Assert.Equal(taskRef.TaskRef.TaskKey, readTaskRef.KLEId);
            Assert.Equal(taskRef.TaskRef.Description, readTaskRef.KLEName);

            // From Parent System
            Assert.Equal(systemParentName, readModel.ParentItSystemName);
            Assert.Equal(systemParent.Id, readModel.ParentItSystemId);
            Assert.Equal(systemParent.Uuid, readModel.ParentItSystemUuid);
            Assert.Equal(systemParentDisabled, readModel.ParentItSystemDisabled);
            Assert.Equal(systemParentUsage.Uuid, readModel.ParentItSystemUsageUuid);

            // Role assignment
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Console.Out.WriteLine("Found one role assignment as expected");

            Assert.Equal(role.Id, roleAssignment.RoleId);
            Assert.Equal(DatabaseAccess.GetEntityUuid<ItSystemRole>(role.Id), roleAssignment.RoleUuid);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.FullName, roleAssignment.UserFullName);
            Assert.Equal(user.Email, roleAssignment.Email);

            // Responsible Organization Unit
            Assert.Equal(responsibleUnit.Uuid, readModel.ResponsibleOrganizationUnitUuid);
            Assert.Equal(responsibleUnit.Id, readModel.ResponsibleOrganizationUnitId);
            Assert.Equal(responsibleUnit.Name, readModel.ResponsibleOrganizationUnitName);
            Assert.Contains(readModel.RelevantOrganizationUnits, orgUnitReadModel => MatchExpectedOrgUnit(orgUnitReadModel, organizationUnit1));
            Assert.Contains(readModel.RelevantOrganizationUnits, orgUnitReadModel => MatchExpectedOrgUnit(orgUnitReadModel, organizationUnit2));

            // Reference
            Assert.Equal(reference.Title, readModel.LocalReferenceTitle);
            Assert.Equal(reference.URL, readModel.LocalReferenceUrl);
            Assert.Equal(reference.ExternalReferenceId, readModel.LocalReferenceDocumentId);

            // Main Contract
            Assert.Equal(contract1.Id, readModel.MainContractId);
            Assert.Equal(organizationId, readModel.MainContractSupplierId);
            Assert.Equal(organizationName, readModel.MainContractSupplierName);
            Assert.True(readModel.MainContractIsActive);

            // Associated contracts
            var expectedContracts = new[] { contract1, contract2 }.ToList();
            Assert.Equal(expectedContracts.Count, readModel.AssociatedContracts.Count);
            foreach (var expectedContract in expectedContracts)
            {
                Assert.Contains(expectedContract.Name, readModel.AssociatedContractsNamesCsv);
                Assert.Contains(readModel.AssociatedContracts, c => c.ItContractId == expectedContract.Id);
            }

            // ArchivePeriods
            Assert.Equal(archivePeriodEndDate, readModel.ActiveArchivePeriodEndDate);
            var rmArchivePeriod = Assert.Single(readModel.ArchivePeriods);
            Assert.Equal(archivePeriodStartDate, rmArchivePeriod.StartDate);
            Assert.Equal(archivePeriodEndDate, rmArchivePeriod.EndDate);

            // DataProcessingRegistration
            Assert.Equal(dataProcessingRegistration.Name, readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal(yesNoIrrelevantOption.GetReadableName(), readModel.DataProcessingRegistrationsConcludedAsCsv);
            var dpr = Assert.Single(readModel.DataProcessingRegistrations);
            Assert.Equal(dataProcessingRegistration.Uuid, dpr.DataProcessingRegistrationUuid);

            // DependsOnInterfaces 
            Assert.Equal(relationInterfaceName, readModel.DependsOnInterfacesNamesAsCsv);
            var rmDependsOnInterface = Assert.Single(readModel.DependsOnInterfaces);
            Assert.Equal(relationInterface.Id, rmDependsOnInterface.InterfaceId);
            Assert.Equal(relationInterface.Uuid, rmDependsOnInterface.InterfaceUuid);
            Assert.Equal(relationInterfaceName, rmDependsOnInterface.InterfaceName);

            //Incoming system usages
            Assert.Equal(incomingRelationSystemName, readModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            var rmIncomingRelatedItSystemUsage = Assert.Single(readModel.IncomingRelatedItSystemUsages);
            Assert.Equal(incomingRelationSystemUsage.Id, rmIncomingRelatedItSystemUsage.ItSystemUsageId);
            Assert.Equal(incomingRelationSystemName, rmIncomingRelatedItSystemUsage.ItSystemUsageName);

            //Outgoing system usages
            Assert.Equal(outgoingRelationSystemName, readModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
            var rmOutgoingRelatedItSystemUsage = Assert.Single(readModel.OutgoingRelatedItSystemUsages);
            Assert.Equal(outgoingRelationSystemUsage.Id, rmOutgoingRelatedItSystemUsage.ItSystemUsageId);
            Assert.Equal(outgoingRelationSystemUsage.Uuid, rmOutgoingRelatedItSystemUsage.ItSystemUsageUuid);
            Assert.Equal(outgoingRelationSystemName, rmOutgoingRelatedItSystemUsage.ItSystemUsageName);
        }

        [Fact]
        public async Task ReadModels_ItSystemParentName_Is_Null_When_No_Parent()
        {
            //Arrange
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Null(readModel.ParentItSystemName);
            Assert.Null(readModel.ParentItSystemId);
        }

        [Fact]
        public async Task ReadModels_ItSystemParentName_Is_Updated_When_Parent_Name_Is_Updated()
        {
            //Arrange
            var systemName = A<string>();
            var systemParentName = A<string>();
            var newSystemParentName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemParent = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemParentName, organizationId, AccessModifier.Public);
            await ItSystemHelper.SendSetParentSystemRequestAsync(system.Id, systemParent.Id, organizationId).DisposeAsync();
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemHelper.SendSetNameRequestAsync(systemParent.Id, newSystemParentName, organizationId).DisposeAsync();
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newSystemParentName, readModel.ParentItSystemName);
            Assert.Equal(systemParent.Id, readModel.ParentItSystemId);
        }

        [Fact]
        public async Task ReadModels_ResponsibleOrganizationUnit_Is_Updated_When_ResponsibleOrganizationUnit_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var orgUnitName1 = A<string>();
            var orgUnitName2 = A<string>();
            var organizationUnit1 = await OrganizationHelper.CreateOrganizationUnitAsync(organizationId, orgUnitName1);
            var organizationUnit2 = await OrganizationHelper.CreateOrganizationUnitAsync(organizationId, orgUnitName2);

            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit1.Id, organizationId).DisposeAsync();
            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit2.Id, organizationId).DisposeAsync();
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit1.Id).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit2.Id).DisposeAsync();
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(organizationUnit2.Id, readModel.ResponsibleOrganizationUnitId);
            Assert.Equal(orgUnitName2, readModel.ResponsibleOrganizationUnitName);
        }

        [Fact]
        public async Task ReadModels_ItSystemRightsHolderName_Is_Updated_When_OrganizationName_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var organizationName1 = A<string>();
            var organizationName2 = A<string>();
            var defaultOrganizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, defaultOrganizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, defaultOrganizationId);

            var organization1 = await OrganizationHelper.CreateOrganizationAsync(defaultOrganizationId, organizationName1, "", OrganizationTypeKeys.Kommune, AccessModifier.Public);

            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, organization1.Id, defaultOrganizationId).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await OrganizationHelper.SendChangeOrganizationNameRequestAsync(organization1.Id, organizationName2, defaultOrganizationId).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(defaultOrganizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(organizationName2, readModel.ItSystemRightsHolderName);
        }

        [Fact]
        public async Task ReadModels_ItSystemBusinessTypeName_Is_Updated_When_BusinessType_Has_Its_Name_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var businessTypeName1 = A<string>();
            var businessTypeName2 = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var businessType = await EntityOptionHelper.CreateOptionTypeAsync(EntityOptionHelper.ResourceNames.BusinessType, businessTypeName1, organizationId);

            await ItSystemHelper.SendSetBusinessTypeRequestAsync(system.Id, businessType.Id, organizationId).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await EntityOptionHelper.ChangeOptionTypeNameAsync(EntityOptionHelper.ResourceNames.BusinessType, businessType.Id, businessTypeName2);
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(businessTypeName2, readModel.ItSystemBusinessTypeName);
        }

        [Fact]
        public async Task ReadModels_MainContractName_And_MainContractSupplierName_Is_Updated_When_MainContract_Is_Deleted()
        {
            //Arrange
            var systemName = A<string>();
            var contractName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var contract = await ItContractHelper.CreateContract(contractName, organizationId);

            var contractUpdateBody = new
            {
                supplierId = organizationId
            };
            await ItContractHelper.PatchContract(contract.Id, organizationId, contractUpdateBody);
            await ItContractHelper.AddItSystemUsage(contract.Id, systemUsage.Id, organizationId);

            await ItSystemUsageHelper.SendSetMainContractRequestAsync(systemUsage.Id, contract.Id).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItContractHelper.SendDeleteContractRequestAsync(contract.Id).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");
            Assert.Empty(readModel.AssociatedContracts);
            Assert.Equal(string.Empty, readModel.AssociatedContractsNamesCsv);
            Assert.Null(readModel.MainContractId);
            Assert.Null(readModel.MainContractSupplierId);
            Assert.Null(readModel.MainContractSupplierName);
            Assert.True(readModel.MainContractIsActive);
        }

        [Fact]
        public async Task ReadModels_DataProcessingRegistrations_Is_Updated_When_DataProcessingRegistration_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var dataProcessingRegistrationName = A<string>();
            var newDataProcessingRegistrationName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var yesNoIrrelevantOption = A<YesNoIrrelevantOption>();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var dataProcessingRegistration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, dataProcessingRegistrationName);
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dataProcessingRegistration.Id, A<YesNoIrrelevantOption>()).DisposeAsync();
            await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dataProcessingRegistration.Id, systemUsage.Id).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await DataProcessingRegistrationHelper.SendChangeNameRequestAsync(dataProcessingRegistration.Id, newDataProcessingRegistrationName).DisposeAsync();
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dataProcessingRegistration.Id, yesNoIrrelevantOption).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newDataProcessingRegistrationName, readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal(yesNoIrrelevantOption.GetReadableName(), readModel.DataProcessingRegistrationsConcludedAsCsv);
            var rmDataProcessingRegistration = Assert.Single(readModel.DataProcessingRegistrations);
            Assert.Equal(dataProcessingRegistration.Id, rmDataProcessingRegistration.DataProcessingRegistrationId);
            Assert.Equal(newDataProcessingRegistrationName, rmDataProcessingRegistration.DataProcessingRegistrationName);
            Assert.Equal(yesNoIrrelevantOption, rmDataProcessingRegistration.IsAgreementConcluded);
        }

        [Fact]
        public async Task ReadModels_DependsOnInterfacesNamesAsCsv_Is_Updated_When_Interface_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var relationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var newRelationInterfaceName = A<string>();
            var relationInterfaceId = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var relationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(relationSystemName, organizationId, AccessModifier.Public);
            var relationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(relationSystem.Id, organizationId);

            var relationInterfaceDTO = InterfaceHelper.CreateInterfaceDto(relationInterfaceName, relationInterfaceId, organizationId, AccessModifier.Public);
            var relationInterface = await InterfaceHelper.CreateInterface(relationInterfaceDTO);
            await InterfaceExhibitHelper.CreateExhibit(relationSystem.Id, relationInterface.Id);

            var outgoingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = systemUsage.Id,
                ToUsageId = relationSystemUsage.Id,
                InterfaceId = relationInterface.Id
            };
            await SystemRelationHelper.PostRelationAsync(outgoingRelationDTO);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await InterfaceHelper.SendChangeNameRequestAsync(relationInterface.Id, newRelationInterfaceName, organizationId).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");


            //Assert
            var mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newRelationInterfaceName, mainSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.IncomingRelatedItSystemUsages);
            var rmInterface = Assert.Single(mainSystemReadModel.DependsOnInterfaces);
            Assert.Equal(relationInterface.Id, rmInterface.InterfaceId);
            Assert.Equal(newRelationInterfaceName, rmInterface.InterfaceName);


            var relationSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, relationSystemName, 1, 0)).ToList();
            var relationSystemReadModel = Assert.Single(relationSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(systemName, relationSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.DependsOnInterfaces);
            var rmIncomingSystemUsage = Assert.Single(relationSystemReadModel.IncomingRelatedItSystemUsages);
            Assert.Equal(systemUsage.Id, rmIncomingSystemUsage.ItSystemUsageId);
            Assert.Equal(systemName, rmIncomingSystemUsage.ItSystemUsageName);
        }

        [Fact]
        public async Task ReadModels_DependsOnInterfacesNamesAsCsv_Is_Updated_When_SystemRelation_Is_Deleted()
        {
            //Arrange
            var systemName = A<string>();
            var relationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var relationInterfaceId = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var relationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(relationSystemName, organizationId, AccessModifier.Public);
            var relationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(relationSystem.Id, organizationId);

            var relationInterfaceDTO = InterfaceHelper.CreateInterfaceDto(relationInterfaceName, relationInterfaceId, organizationId, AccessModifier.Public);
            var relationInterface = await InterfaceHelper.CreateInterface(relationInterfaceDTO);
            await InterfaceExhibitHelper.CreateExhibit(relationSystem.Id, relationInterface.Id);

            var outgoingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = systemUsage.Id,
                ToUsageId = relationSystemUsage.Id,
                InterfaceId = relationInterface.Id
            };
            var relation = await SystemRelationHelper.PostRelationAsync(outgoingRelationDTO);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await SystemRelationHelper.SendDeleteRelationRequestAsync(systemUsage.Id, relation.Id).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Assert
            var mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal("", mainSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.DependsOnInterfaces);
            Assert.Equal("", mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.IncomingRelatedItSystemUsages);


            var relationSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, relationSystemName, 1, 0)).ToList();
            var relationSystemReadModel = Assert.Single(relationSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal("", relationSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.IncomingRelatedItSystemUsages);
            Assert.Equal("", relationSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.DependsOnInterfaces);
        }

        [Fact]
        public async Task ReadModels_DependsOnInterfacesNamesAsCsv_And_OutGoingRelations_Is_Updated_When_SystemRelation_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var relationSystemName = A<string>();
            var newRelationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var relationInterfaceId = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var relationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(relationSystemName, organizationId, AccessModifier.Public);
            var relationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(relationSystem.Id, organizationId);

            var newRelationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(newRelationSystemName, organizationId, AccessModifier.Public);
            var newRelationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newRelationSystem.Id, organizationId);

            var relationInterfaceDTO = InterfaceHelper.CreateInterfaceDto(relationInterfaceName, relationInterfaceId, organizationId, AccessModifier.Public);
            var relationInterface = await InterfaceHelper.CreateInterface(relationInterfaceDTO);
            await InterfaceExhibitHelper.CreateExhibit(relationSystem.Id, relationInterface.Id);

            var outgoingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = systemUsage.Id,
                ToUsageId = relationSystemUsage.Id,
                InterfaceId = relationInterface.Id
            };
            var relation = await SystemRelationHelper.PostRelationAsync(outgoingRelationDTO);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            var newRelationAsNamedEntity = new NamedEntityWithEnabledStatusDTO(newRelationSystemUsage.Id, newRelationSystemName, false);
            var newOutgoingRelationDTO = new SystemRelationDTO(
                relation.Id,
                relation.Uuid,
                relation.FromUsage,
                newRelationAsNamedEntity,
                null, //Interface is not exposed by new system so it needs to be reset
                relation.Contract,
                relation.FrequencyType,
                relation.Description,
                relation.Reference);

            await SystemRelationHelper.SendPatchRelationRequestAsync(newOutgoingRelationDTO).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Assert
            var mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newRelationSystemName, mainSystemReadModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
            Assert.Equal("", mainSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.DependsOnInterfaces);
            Assert.Equal("", mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.IncomingRelatedItSystemUsages);


            var relationSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, relationSystemName, 1, 0)).ToList();
            var relationSystemReadModel = Assert.Single(relationSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal("", relationSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.IncomingRelatedItSystemUsages);
            Assert.Equal("", relationSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.DependsOnInterfaces);


            var newRelationSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, newRelationSystemName, 1, 0)).ToList();
            var newRelationSystemReadModel = Assert.Single(newRelationSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(systemName, newRelationSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            var rmSystemUsage = Assert.Single(newRelationSystemReadModel.IncomingRelatedItSystemUsages);
            Assert.Equal(systemUsage.Id, rmSystemUsage.ItSystemUsageId);
            Assert.Equal(systemName, rmSystemUsage.ItSystemUsageName);
            Assert.Equal("", newRelationSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(newRelationSystemReadModel.DependsOnInterfaces);
        }

        [Fact]
        public async Task ReadModels_Relations_Are_Is_Updated_When_AffectedSystemUsageIsRemoved()
        {
            //Arrange
            var systemName = A<string>();
            var relationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var relationInterfaceId = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var relationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(relationSystemName, organizationId, AccessModifier.Public);
            var relationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(relationSystem.Id, organizationId);

            var relationInterfaceDTO = InterfaceHelper.CreateInterfaceDto(relationInterfaceName, relationInterfaceId, organizationId, AccessModifier.Public);
            var relationInterface = await InterfaceHelper.CreateInterface(relationInterfaceDTO);
            await InterfaceExhibitHelper.CreateExhibit(relationSystem.Id, relationInterface.Id);

            var outgoingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = systemUsage.Id,
                ToUsageId = relationSystemUsage.Id,
                InterfaceId = relationInterface.Id
            };

            var incomingRelationDto = new CreateSystemRelationDTO
            {
                FromUsageId = relationSystemUsage.Id,
                ToUsageId = systemUsage.Id
            };
            await SystemRelationHelper.PostRelationAsync(outgoingRelationDTO);
            await SystemRelationHelper.PostRelationAsync(incomingRelationDto);

            //Await first update
            await WaitForReadModelQueueDepletion();

            //Act - Second update should blank out the relation fields since the affected usage has been killed
            using var removeUsage = await ItSystemHelper.SendRemoveUsageAsync(relationSystemUsage.Id, organizationId);
            Assert.Equal(HttpStatusCode.OK, removeUsage.StatusCode);

            //Assert
            await WaitForReadModelQueueDepletion();
            var mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(string.Empty, mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Equal(string.Empty, mainSystemReadModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.OutgoingRelatedItSystemUsages);
            Assert.Empty(mainSystemReadModel.IncomingRelatedItSystemUsages);
        }

        [Fact]
        public async Task ReadModels_Relations_Are_Updated_When_Participating_Systems_Change_Name()
        {
            //Arrange
            var systemName = A<string>();
            var outgoingRelationSystemName_initial = A<string>();
            var outgoingRelationSystemName_changed = $"{outgoingRelationSystemName_initial}_1";
            var incomingRelationSystemName_initial = A<string>();
            var incomingRelationSystemName_changed = $"{incomingRelationSystemName_initial}_1";
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var outGoingRelationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(outgoingRelationSystemName_initial, organizationId, AccessModifier.Public);
            var outGoingRelationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(outGoingRelationSystem.Id, organizationId);

            var incomingRelationSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(incomingRelationSystemName_initial, organizationId, AccessModifier.Public);
            var incomingRelationSystemUsage = await ItSystemHelper.TakeIntoUseAsync(incomingRelationSystem.Id, organizationId);

            var outgoingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = systemUsage.Id,
                ToUsageId = outGoingRelationSystemUsage.Id,
            };

            var incomingRelationDTO = new CreateSystemRelationDTO
            {
                FromUsageId = incomingRelationSystemUsage.Id,
                ToUsageId = systemUsage.Id,
            };


            await SystemRelationHelper.PostRelationAsync(outgoingRelationDTO);
            await SystemRelationHelper.PostRelationAsync(incomingRelationDTO);

            //Await first update
            await WaitForReadModelQueueDepletion();

            //Act + assert - Rename the system used in incoming relation and verify that the readmodel is updated
            using var renameIncomingSystem = await ItSystemHelper.SendSetNameRequestAsync(incomingRelationSystem.Id, incomingRelationSystemName_changed, organizationId);
            Assert.Equal(HttpStatusCode.OK, renameIncomingSystem.StatusCode);

            await WaitForReadModelQueueDepletion();
            var mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Assert.Equal(incomingRelationSystemName_changed, mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);

            //Act + assert - Rename the system used in outgoing relation and verify that the readmodel is updated
            using var renameOutgoingSystem = await ItSystemHelper.SendSetNameRequestAsync(outGoingRelationSystem.Id, outgoingRelationSystemName_changed, organizationId);
            Assert.Equal(HttpStatusCode.OK, renameOutgoingSystem.StatusCode);
            await WaitForReadModelQueueDepletion();

            mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Assert.Equal(outgoingRelationSystemName_changed, mainSystemReadModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
        }

        [Fact]
        public async Task When_SystemRightIsDeleted_Role_Assignment_In_Readmodel_Is_Updated()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var systemName = A<string>();
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            // Role assignment
            var businessRoleDtos = await ItSystemUsageHelper.GetAvailableRolesAsync(organizationId);
            var role = businessRoleDtos.First();
            var availableUsers = await ItSystemUsageHelper.GetAvailableUsersAsync(organizationId);
            var user = availableUsers.First();
            using var assignRoleResponse = await ItSystemUsageHelper.SendAssignRoleRequestAsync(systemUsage.Id, organizationId, role.Id, user.Id);
            Assert.Equal(HttpStatusCode.Created, assignRoleResponse.StatusCode);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            await Console.Out.WriteLineAsync("Read models are up to date");

            //Act 
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            await Console.Out.WriteLineAsync("Read model found");
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            await Console.Out.WriteLineAsync("Found one role assignment as expected");

            //Act - remove the right using the odata api
            var rightId = DatabaseAccess.MapFromEntitySet<ItSystemRight, int>(rights => rights.AsQueryable().Single(x => x.ObjectId == readModel.SourceEntityId).Id);
            await ItSystemUsageHelper.SendOdataDeleteRightRequestAsync(rightId).WithExpectedResponseCode(HttpStatusCode.NoContent).DisposeAsync();

            //Assert
            await WaitForReadModelQueueDepletion();
            await Console.Out.WriteLineAsync("Read models are up to date");

            readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            readModel = Assert.Single(readModels);
            Assert.Empty(readModel.RoleAssignments);
        }

        private static async Task WaitForReadModelQueueDepletion()
        {
            await ReadModelTestTools.WaitForReadModelQueueDepletion();
        }

        private static bool MatchExpectedOrgUnit(ItSystemUsageOverviewRelevantOrgUnitReadModel x, OrgUnitDTO organizationUnit1)
        {
            return x.OrganizationUnitId == organizationUnit1.Id && x.OrganizationUnitUuid == organizationUnit1.Uuid && x.OrganizationUnitName == organizationUnit1.Name;
        }

        private static async Task<ItSystemResponseDTO> PrepareItSystem(string systemName, string systemPreviousName, string systemDescription, int organizationId, string organizationName, AccessModifier accessModifier)
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(organizationId, organizationName,
                TestCvr, OrganizationTypeKeys.Virksomhed, accessModifier);
            var token = (await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin)).Token;
            var system = await ItSystemV2Helper.CreateSystemAsync(token, new CreateItSystemRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = systemName,
                PreviousName = systemPreviousName,
                Description = systemDescription
            });
            return system;
        }
    }
}
