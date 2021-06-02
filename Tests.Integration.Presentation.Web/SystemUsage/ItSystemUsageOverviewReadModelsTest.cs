using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Models;
using Presentation.Web.Models.SystemRelations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.SystemUsage
{
    public class ItSystemUsageOverviewReadModelsTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Query_And_Page_ReadModels()
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
            var page1 = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 0)).ToList();
            var page2 = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 2)).ToList();

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
            var systemDisabled = A<bool>();

            var systemParentName = A<string>();
            var systemParentDisabled = A<bool>();

            var systemUsageActive = A<bool>();
            var systemUsageExpirationDate = DateTime.Now.AddDays(-1);
            var systemUsageVersion = A<string>();
            var systemUsageLocalCallName = A<string>();
            var systemUsageLocalSystemId = A<string>();
            var concluded = A<DateTime>();
            var archiveDuty = A<ArchiveDutyTypes>();
            var riskAssessment = A<DataOptions>();
            var linkToDirectoryUrl = A<string>();
            var linkToDirectoryUrlName = A<string>();
            var riskSupervisionDocumentationUrl = A<string>();
            var riskSupervisionDocumentationUrlName = A<string>();
            var generalPurpose = A<string>();
            var hostedAt = A<HostedAt>();

            var contractName = A<string>();

            var projectName = A<string>();

            var dataProcessingRegistrationName = A<string>();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemParent = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemParentName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            // Role assignment
            var businessRoleDtos = await ItSystemUsageHelper.GetAvailableRolesAsync(organizationId);
            var role = businessRoleDtos.First();
            var availableUsers = await ItSystemUsageHelper.GetAvailableUsersAsync(organizationId);
            var user = availableUsers.First();
            using var assignRoleResponse = await ItSystemUsageHelper.SendAssignRoleRequestAsync(systemUsage.Id, organizationId, role.Id, user.Id);
            Assert.Equal(HttpStatusCode.Created, assignRoleResponse.StatusCode);

            // System changes
            await ItSystemHelper.SendSetDisabledRequestAsync(system.Id, systemDisabled);
            await ItSystemHelper.SendSetParentSystemRequestAsync(system.Id, systemParent.Id, organizationId);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, organizationId, organizationId); // Using default organization as BelongsTo

            var availableBusinessTypeOptions = (await ItSystemHelper.GetBusinessTypeOptionsAsync(organizationId)).ToList();
            var businessType = availableBusinessTypeOptions[Math.Abs(A<int>()) % availableBusinessTypeOptions.Count];
            await ItSystemHelper.SendSetBusinessTypeRequestAsync(system.Id, businessType.Id, organizationId);

            var taskRefs = (await ItSystemHelper.GetAvailableTaskRefsRequestAsync(system.Id)).ToList();
            var taskRef = taskRefs[Math.Abs(A<int>()) % taskRefs.Count];
            await ItSystemHelper.SendAddTaskRefRequestAsync(system.Id, taskRef.TaskRef.Id, organizationId);

            // Parent system 
            await ItSystemHelper.SendSetDisabledRequestAsync(systemParent.Id, systemParentDisabled);

            // System Usage changes
            var body = new
            {
                Active = systemUsageActive,
                ExpirationDate = systemUsageExpirationDate,
                Version = systemUsageVersion,
                LocalCallName = systemUsageLocalCallName,
                LocalSystemId = systemUsageLocalSystemId,
                Concluded = concluded,
                ArchiveDuty = archiveDuty,
                RiskAssessment = riskAssessment,
                linkToDirectoryUrl = linkToDirectoryUrl,
                linkToDirectoryUrlName = linkToDirectoryUrlName,
                riskSupervisionDocumentationUrl = riskSupervisionDocumentationUrl,
                riskSupervisionDocumentationUrlName = riskSupervisionDocumentationUrlName,
                GeneralPurpose = generalPurpose,
                HostedAt = hostedAt
            };
            await ItSystemUsageHelper.PatchSystemUsage(systemUsage.Id, organizationId, body);
            var sensitiveDataLevel = await ItSystemUsageHelper.AddSensitiveDataLevel(systemUsage.Id, A<SensitiveDataLevel>());
            var isHoldingDocument = A<bool>();
            await ItSystemUsageHelper.SetIsHoldingDocumentRequestAsync(systemUsage.Id, isHoldingDocument);

            // Responsible Organization Unit
            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationId, organizationId); //Adding default organization as organization unit
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationId); //Using default organization as responsible organization unit

            //References
            var reference = await ReferencesHelper.CreateReferenceAsync(A<string>(), A<string>(), A<string>(), A<Display>(), dto => dto.ItSystemUsage_Id = systemUsage.Id);

            //Main Contract
            var contract = await ItContractHelper.CreateContract(contractName, organizationId);
            var contractUpdateBody = new
            {
                supplierId = organizationId
            };
            await ItContractHelper.PatchContract(contract.Id, organizationId, contractUpdateBody);
            await ItContractHelper.AddItSystemUsage(contract.Id, systemUsage.Id, organizationId);

            await ItSystemUsageHelper.SendSetMainContractRequestAsync(systemUsage.Id, contract.Id);

            // Project
            var project = await ItProjectHelper.CreateProject(projectName, organizationId);
            await ItProjectHelper.AddSystemBinding(project.Id, systemUsage.Id, organizationId);

            // ArchivePeriods
            var archivePeriodStartDate = DateTime.Now.AddDays(-1);
            var archivePeriodEndDate = DateTime.Now.AddDays(1);
            var archivePeriod = await ItSystemUsageHelper.SendAddArchivePeriodRequestAsync(systemUsage.Id, archivePeriodStartDate, archivePeriodEndDate, organizationId);


            // DataProcessingRegistrations
            var yesNoIrrelevantOption = A<YesNoIrrelevantOption>();
            var dataProcessingRegistration = await DataProcessingRegistrationHelper.CreateAsync(organizationId, dataProcessingRegistrationName);
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dataProcessingRegistration.Id, yesNoIrrelevantOption);
            await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dataProcessingRegistration.Id, systemUsage.Id);

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
            Assert.Equal(organizationId, readModel.OrganizationId);
            Assert.Equal(systemUsageActive, readModel.IsActive);
            Assert.Equal(systemUsageVersion, readModel.Version);
            Assert.Equal(systemUsageLocalCallName, readModel.LocalCallName);
            Assert.Equal(updatedSystemUsage.ObjectOwnerFullName, readModel.ObjectOwnerName);
            Assert.Equal(updatedSystemUsage.ObjectOwnerFullName, readModel.LastChangedByName); // Same user was used to create and change the systemUsage
            Assert.Equal(concluded, readModel.Concluded);
            Assert.Equal(updatedSystemUsage.LastChanged, readModel.LastChangedAt);
            Assert.Equal(archiveDuty, readModel.ArchiveDuty);
            Assert.Equal(isHoldingDocument, readModel.IsHoldingDocument);
            Assert.Equal(linkToDirectoryUrlName, readModel.LinkToDirectoryName);
            Assert.Equal(linkToDirectoryUrl, readModel.LinkToDirectoryUrl);
            Assert.Equal(generalPurpose, readModel.GeneralPurpose);
            Assert.Equal(hostedAt, readModel.HostedAt);

            if (riskAssessment == DataOptions.YES)
            {
                Assert.Equal(riskSupervisionDocumentationUrlName, readModel.RiskSupervisionDocumentationName);
                Assert.Equal(riskSupervisionDocumentationUrl, readModel.RiskSupervisionDocumentationUrl);
            }
            else
            {
                Assert.Null(readModel.RiskSupervisionDocumentationName);
                Assert.Null(readModel.RiskSupervisionDocumentationUrl);
            }

            // Sensitive Data Level
            var rmSensitiveDataLevel = Assert.Single(readModel.SensitiveDataLevels);
            Assert.Equal(sensitiveDataLevel.DataSensitivityLevel, rmSensitiveDataLevel.SensitivityDataLevel);
            Assert.Equal(sensitiveDataLevel.DataSensitivityLevel.GetReadableName(), readModel.SensitiveDataLevelsAsCsv);

            // From System
            Assert.Equal(systemName, readModel.SystemName);
            Assert.Equal(systemDisabled, readModel.ItSystemDisabled);
            Assert.Equal(system.Uuid.ToString("D"), readModel.ItSystemUuid);
            Assert.Equal(businessType.Id, readModel.ItSystemBusinessTypeId);
            Assert.Equal(businessType.Name, readModel.ItSystemBusinessTypeName);
            Assert.Equal(organizationId, readModel.ItSystemRightsHolderId);
            Assert.Equal(organizationName, readModel.ItSystemRightsHolderName);
            Assert.Equal(taskRef.TaskRef.TaskKey, readModel.ItSystemKLEIdsAsCsv);
            Assert.Equal(taskRef.TaskRef.Description, readModel.ItSystemKLENamesAsCsv);
            var readTaskRef = Assert.Single(readModel.ItSystemTaskRefs);
            Assert.Equal(taskRef.TaskRef.TaskKey, readTaskRef.KLEId);
            Assert.Equal(taskRef.TaskRef.Description, readTaskRef.KLEName);

            // From Parent System
            Assert.Equal(systemParentName, readModel.ParentItSystemName);
            Assert.Equal(systemParent.Id, readModel.ParentItSystemId);
            Assert.Equal(systemParentDisabled, readModel.ParentItSystemDisabled);

            // Role assignment
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Console.Out.WriteLine("Found one role assignment as expected");

            Assert.Equal(role.Id, roleAssignment.RoleId);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.FullName, roleAssignment.UserFullName);
            Assert.Equal(user.Email, roleAssignment.Email);

            // Responsible Organization Unit
            Assert.Equal(organizationId, readModel.ResponsibleOrganizationUnitId);
            Assert.Equal(organizationName, readModel.ResponsibleOrganizationUnitName);

            // Reference
            Assert.Equal(reference.Title, readModel.LocalReferenceTitle);
            Assert.Equal(reference.URL, readModel.LocalReferenceUrl);
            Assert.Equal(reference.ExternalReferenceId, readModel.LocalReferenceDocumentId);

            // Main Contract
            Assert.Equal(contract.Id, readModel.MainContractId);
            Assert.Equal(organizationId, readModel.MainContractSupplierId);
            Assert.Equal(organizationName, readModel.MainContractSupplierName);
            Assert.True(readModel.MainContractIsActive);
            Assert.True(readModel.HasMainContract);

            // Project
            Assert.Equal(projectName, readModel.ItProjectNamesAsCsv);
            var rmProject = Assert.Single(readModel.ItProjects);
            Assert.Equal(project.Id, rmProject.ItProjectId);
            Assert.Equal(projectName, rmProject.ItProjectName);

            // ArchivePeriods
            Assert.Equal(archivePeriodEndDate, readModel.ActiveArchivePeriodEndDate);
            var rmArchivePeriod = Assert.Single(readModel.ArchivePeriods);
            Assert.Equal(archivePeriodStartDate, rmArchivePeriod.StartDate);
            Assert.Equal(archivePeriodEndDate, rmArchivePeriod.EndDate);

            // DataProcessingRegistration
            Assert.Equal(dataProcessingRegistration.Name, readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal(yesNoIrrelevantOption.GetReadableName(), readModel.DataProcessingRegistrationsConcludedAsCsv);
            Assert.Single(readModel.DataProcessingRegistrations);

            // DependsOnInterfaces 
            Assert.Equal(relationInterfaceName, readModel.DependsOnInterfacesNamesAsCsv);
            var rmDependsOnInterface = Assert.Single(readModel.DependsOnInterfaces);
            Assert.Equal(relationInterface.Id, rmDependsOnInterface.InterfaceId);
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
            Assert.Equal(outgoingRelationSystemName, rmOutgoingRelatedItSystemUsage.ItSystemUsageName);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_True_When_ExpirationDate_Is_Today()
        {
            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(DateTime.Now);

            //Assert
            Assert.True(readModel.IsActive);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_True_When_ExpirationDate_Is_After_Today()
        {
            //Arrange
            var expirationDate = DateTime.Now.AddDays(A<int>());

            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(expirationDate);

            //Assert
            Assert.True(readModel.IsActive);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_False_When_ExpirationDate_Is_Earlier_Than_Today()
        {
            //Arrange
            var expirationDate = DateTime.Now.AddDays(-A<int>());

            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(expirationDate);

            //Assert
            Assert.False(readModel.IsActive);
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
            await ItSystemHelper.SendSetParentSystemRequestAsync(system.Id, systemParent.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemHelper.SendSetNameRequestAsync(systemParent.Id, newSystemParentName, organizationId);
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
            var orgUnitName1 = A<string>();
            var orgUnitName2 = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var organizationUnit1 = await OrganizationHelper.SendCreateOrganizationUnitRequestAsync(organizationId, orgUnitName1);
            var organizationUnit2 = await OrganizationHelper.SendCreateOrganizationUnitRequestAsync(organizationId, orgUnitName2);

            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit1.Id, organizationId);
            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit2.Id, organizationId);
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit1.Id);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit2.Id);
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

            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, organization1.Id, defaultOrganizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await OrganizationHelper.SendChangeOrganizationNameRequestAsync(organization1.Id, organizationName2, defaultOrganizationId);
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

            var businessType = await EntityOptionHelper.SendCreateBusinessTypeAsync(businessTypeName1, organizationId);

            await ItSystemHelper.SendSetBusinessTypeRequestAsync(system.Id, businessType.Id, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await EntityOptionHelper.SendChangeBusinessTypeNameAsync(businessType.Id, businessTypeName2);
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

            await ItSystemUsageHelper.SendSetMainContractRequestAsync(systemUsage.Id, contract.Id);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItContractHelper.SendDeleteContractRequestAsync(contract.Id);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Null(readModel.MainContractId);
            Assert.Null(readModel.MainContractSupplierId);
            Assert.Null(readModel.MainContractSupplierName);
            Assert.Null(readModel.MainContractIsActive);
            Assert.False(readModel.HasMainContract);
        }

        [Fact]
        public async Task ReadModels_ItProjects_Is_Updated_When_ItProject_Is_Deleted()
        {
            //Arrange
            var systemName = A<string>();
            var projectName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var project = await ItProjectHelper.CreateProject(projectName, organizationId);
            await ItProjectHelper.AddSystemBinding(project.Id, systemUsage.Id, organizationId);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItProjectHelper.SendDeleteProjectAsync(project.Id);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal("", readModel.ItProjectNamesAsCsv);
            Assert.Empty(readModel.ItProjects);
        }

        [Fact]
        public async Task ReadModels_ItProjects_Is_Updated_When_ItProject_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var projectName = A<string>();
            var newProjectName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var project = await ItProjectHelper.CreateProject(projectName, organizationId);
            await ItProjectHelper.AddSystemBinding(project.Id, systemUsage.Id, organizationId);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItProjectHelper.SendChangeNameRequestAsync(project.Id, newProjectName, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newProjectName, readModel.ItProjectNamesAsCsv);
            var rmProject = Assert.Single(readModel.ItProjects);
            Assert.Equal(project.Id, rmProject.ItProjectId);
            Assert.Equal(newProjectName, rmProject.ItProjectName);
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
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dataProcessingRegistration.Id, A<YesNoIrrelevantOption>());
            await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dataProcessingRegistration.Id, systemUsage.Id);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await DataProcessingRegistrationHelper.SendChangeNameRequestAsync(dataProcessingRegistration.Id, newDataProcessingRegistrationName);
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dataProcessingRegistration.Id, yesNoIrrelevantOption);

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
            await InterfaceHelper.SendChangeNameRequestAsync(relationInterface.Id, newRelationInterfaceName, organizationId);

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
            await SystemRelationHelper.SendDeleteRelationRequestAsync(systemUsage.Id, relation.Id);

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

            await SystemRelationHelper.SendPatchRelationRequestAsync(newOutgoingRelationDTO);

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
            Assert.Equal(HttpStatusCode.OK,renameIncomingSystem.StatusCode);

            await WaitForReadModelQueueDepletion();
            var mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Assert.Equal(incomingRelationSystemName_changed,mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);

            //Act + assert - Rename the system used in outgoing relation and verify that the readmodel is updated
            using var renameOutgoingSystem = await ItSystemHelper.SendSetNameRequestAsync(outGoingRelationSystem.Id, outgoingRelationSystemName_changed, organizationId);
            Assert.Equal(HttpStatusCode.OK, renameOutgoingSystem.StatusCode);
            await WaitForReadModelQueueDepletion();

            mainSystemReadModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();
            mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Assert.Equal(outgoingRelationSystemName_changed, mainSystemReadModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
        }

        private async Task<ItSystemUsageOverviewReadModel> Test_For_IsActive_Based_On_ExpirationDate(DateTime expirationDate)
        {
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);
            await ItSystemUsageHelper.SendSetExpirationDateRequestAsync(systemUsage.Id, organizationId, expirationDate);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");


            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            return readModel;
        }

        private static async Task WaitForReadModelQueueDepletion()
        {
            await WaitForAsync(
                () =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<PendingReadModelUpdate, bool>(x => !x.AsQueryable().Any()));
                }, TimeSpan.FromSeconds(120));
        }

        private static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
