using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Extensions;
using Xunit;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.SystemUsage
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItSystemUsageOverviewReadModelsTest : BaseTest
    {
        private const string TestCvr = "11224455";

        [Fact]
        public async Task Can_Query_And_Page_ReadModels_Using_Db_Id()
        {
            await TestQueryAndPaging(async q => await ItSystemUsageV2Helper.QueryReadModelByNameContent(q.orgUuid, q.query, q.top, q.skip));
        }

        [Fact]
        public async Task Can_Query_And_Page_ReadModels_Using_Uuid()
        {
            await TestQueryAndPaging(async q =>
            {
                return await ItSystemUsageV2Helper.QueryReadModelByNameContent(q.orgUuid, q.query, q.top, q.skip);
            });
        }

        private async Task TestQueryAndPaging(Func<(Guid orgUuid, string query, int top, int skip), Task<IEnumerable<ItSystemUsageOverviewReadModel>>> getPage)
        {
            //Arrange
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var organizationUuid = DatabaseAccess.GetEntityUuid<Organization>(organizationId);
            var suffix = A<Guid>().ToString("N");
            var name1 = $"1_{suffix}";
            var name2 = $"2_{suffix}";
            var name3 = $"3_{suffix}";

            var system1 = await CreateItSystemAsync(organizationUuid, name: name1);
            var system2 = await CreateItSystemAsync(organizationUuid, name: name2);
            var system3 = await CreateItSystemAsync(organizationUuid, name: name3);

            await TakeSystemIntoUsageAsync(system1.Uuid, organizationUuid);
            await TakeSystemIntoUsageAsync(system2.Uuid, organizationUuid);
            await TakeSystemIntoUsageAsync(system3.Uuid, organizationUuid);


            //Act
            var page1 = (await getPage((organizationUuid, suffix, 2, 0))).ToList();
            var page2 = (await getPage((organizationUuid, suffix, 2, 2))).ToList();

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
            var organizationUuid = DatabaseAccess.GetEntityUuid<Organization>(organizationId);
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
            var archiveDuty = A<ArchiveDutyChoice>();

            var riskAssessment = A<YesNoDontKnowChoice>();
            var riskAssessmentDate = A<DateTime?>();
            var riskSupervisionDocumentationUrl = A<string>();
            var riskSupervisionDocumentationUrlName = A<string>();
            if (riskAssessment != YesNoDontKnowChoice.Yes)
            {
                riskAssessmentDate = null;
                riskSupervisionDocumentationUrl = null;
                riskSupervisionDocumentationUrlName = null;
            }
            var linkToDirectoryUrl = A<string>();
            var linkToDirectoryUrlName = A<string>();
            var generalPurpose = A<string>();
            var hostedAt = A<HostedAt>();
            var userCount = A<UserCount>();

            var contract1Name = A<string>();
            var contract2Name = A<string>();

            var dataProcessingRegistrationName = A<string>();

            var system = await PrepareItSystem(systemName, systemPreviousName, systemDescription, organizationId, organizationName);
            var systemParent = await CreateItSystemAsync(organizationUuid, name: systemParentName);
            var systemParentUsage = await TakeSystemIntoUsageAsync(systemParent.Uuid, organizationUuid);

            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            // Role assignment
            var role = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRoles,
                organizationUuid);
            var availableUsers = await OrganizationV2Helper.GetUsersInOrganization(organizationUuid);
            var user = availableUsers.First();
            await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(await GetGlobalToken(), systemUsage.Uuid,
                new RoleAssignmentRequestDTO { RoleUuid = role.Uuid, UserUuid = user.Uuid }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var businessType = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.BusinessType, organizationUuid);

            var refs = await KleOptionV2Helper.GetKleNumbersAsync(await GetGlobalToken());
            var taskRef = refs.Payload.RandomItem();
            var sensitiveDataLevel = A<DataSensitivityLevelChoice>();

            // System changes
            var systemChanges = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>(nameof(UpdateItSystemRequestDTO.Deactivated), systemDisabled),
                new KeyValuePair<string, object>(nameof(UpdateItSystemRequestDTO.ParentUuid), systemParent.Uuid),
                new KeyValuePair<string, object>(nameof(UpdateItSystemRequestDTO.RightsHolderUuid), organizationUuid),
                new KeyValuePair<string, object>(nameof(UpdateItSystemRequestDTO.BusinessTypeUuid), businessType.Uuid),
                new KeyValuePair<string, object>(nameof(UpdateItSystemRequestDTO.KLEUuids), taskRef.Uuid.WrapAsEnumerable())
            };
            await ItSystemV2Helper.PatchSystemAsync(await GetGlobalToken(), system.Uuid, systemChanges);

            // Parent system 
            await ItSystemV2Helper.PatchSystemAsync(await GetGlobalToken(), systemParent.Uuid, new KeyValuePair<string, object>(nameof(UpdateItSystemRequestDTO.Deactivated), systemParentDisabled));

            var dataClassification = await
                OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification,
                    organizationUuid);

            await ItSystemUsageV2Helper.SendPatchGDPR(await GetGlobalToken(), systemUsage.Uuid, new GDPRWriteRequestDTO
            {
                Purpose = generalPurpose,
                DirectoryDocumentation = new SimpleLinkDTO { Name = linkToDirectoryUrlName, Url = linkToDirectoryUrl },
                UserSupervisionDocumentation = new SimpleLinkDTO { Name = riskSupervisionDocumentationUrlName, Url = riskSupervisionDocumentationUrl },
                RiskAssessmentConducted = riskAssessment,
                PlannedRiskAssessmentDate = riskAssessmentDate,
                DataSensitivityLevels = sensitiveDataLevel.WrapAsEnumerable()

            }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var isHoldingDocument = A<bool>();

            // Responsible Organization Unit and relevant units
            var orgUnitName1 = A<string>();
            var orgUnitName2 = A<string>();
            var organizationUnit1 = await CreateOrganizationUnitAsync(organizationUuid, orgUnitName1);
            var organizationUnit2 = await CreateOrganizationUnitAsync(organizationUuid, orgUnitName2);
            var units = new List<OrganizationUnitResponseDTO> { organizationUnit1, organizationUnit2 };
            var responsibleUnit = units.RandomItem();

            await ItSystemUsageV2Helper.SendPatchOrganizationalUsage(await GetGlobalToken(), systemUsage.Uuid,
                new OrganizationUsageWriteRequestDTO
                {
                    UsingOrganizationUnitUuids = units.Select(x => x.Uuid).ToList(),
                    ResponsibleOrganizationUnitUuid = responsibleUnit.Uuid
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //References
            var referenceRequestDTO = A<UpdateExternalReferenceDataWriteRequestDTO>();
            referenceRequestDTO.Uuid = null;
            referenceRequestDTO.MasterReference = true;
            using var referenceResponse = await ItSystemUsageV2Helper.SendPatchExternalReferences(await GetGlobalToken(), systemUsage.Uuid,
                referenceRequestDTO.WrapAsEnumerable());
            var reference = (await referenceResponse.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>()).ExternalReferences.Single();

            //Main Contract
            var contract1 = await CreateItContractAsync(organizationUuid, contract1Name);
            var contract2 = await CreateItContractAsync(organizationUuid, contract2Name);

            await ItContractV2Helper.SendPatchContractSupplierAsync(await GetGlobalToken(), contract1.Uuid,
                new ContractSupplierDataWriteRequestDTO { OrganizationUuid = organizationUuid });

            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract1.Uuid,
                systemUsage.Uuid.WrapAsEnumerable());
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract2.Uuid,
                systemUsage.Uuid.WrapAsEnumerable());

            await ItSystemUsageV2Helper.SendPatchGeneral(await GetGlobalToken(), systemUsage.Uuid,
                new GeneralDataUpdateRequestDTO
                {
                    MainContractUuid = contract1.Uuid,
                    SystemVersion = systemUsageVersion,
                    LocalCallName = systemUsageLocalCallName,
                    LocalSystemId = systemUsageLocalSystemId,
                    Validity = new ItSystemUsageValidityWriteRequestDTO
                    {
                        ValidFrom = concluded,
                        ValidTo = systemUsageExpirationDate
                    },
                    NumberOfExpectedUsers = UserIntervalDtoFromUerCount(userCount),
                    DataClassificationUuid = dataClassification.Uuid,
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            // ArchivePeriods
            var archivePeriodStartDate = DateTime.Now.AddDays(-1);
            var archivePeriodEndDate = DateTime.Now.AddDays(1);
            await ItSystemUsageV2Helper.SendPatchArchiving(await GetGlobalToken(), systemUsage.Uuid,
                new ArchivingCreationRequestDTO
                {
                    ArchiveDuty = archiveDuty,
                    JournalPeriods = new List<JournalPeriodDTO> { new JournalPeriodDTO { StartDate = archivePeriodStartDate, EndDate = archivePeriodEndDate, ArchiveId = A<string>() } },
                    DocumentBearing = isHoldingDocument
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();


            // DataProcessingRegistrations
            var yesNoIrrelevantOption = A<YesNoIrrelevantChoice>();
            var dataProcessingRegistration = await CreateDPRAsync(organizationUuid, dataProcessingRegistrationName);
            await DataProcessingRegistrationV2Helper.PatchIsAgreementConcludedAsync(dataProcessingRegistration.Uuid, yesNoIrrelevantOption);
            await DataProcessingRegistrationV2Helper.PatchSystemsAsync(dataProcessingRegistration.Uuid, systemUsage.Uuid.WrapAsEnumerable()).DisposeAsync();

            // DependsOnInterfaces + IncomingSystemUsages + outgoing system usages
            var outgoingRelationSystemName = A<string>();
            var incomingRelationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var relationInterfaceId = A<string>();

            var incomingRelationSystem = await CreateItSystemAsync(organizationUuid, name: incomingRelationSystemName);
            var incomingRelationSystemUsage = await TakeSystemIntoUsageAsync(incomingRelationSystem.Uuid, organizationUuid);

            var outGoingRelationSystem = await CreateItSystemAsync(organizationUuid, name: outgoingRelationSystemName);
            var outgoingRelationSystemUsage = await TakeSystemIntoUsageAsync(outGoingRelationSystem.Uuid, organizationUuid);

            var relationInterface = await InterfaceV2Helper.CreateItInterfaceAsync(await GetGlobalToken(), new CreateItInterfaceRequestDTO
            {
                OrganizationUuid = organizationUuid,
                Name = relationInterfaceName,
                ExposedBySystemUuid = outGoingRelationSystem.Uuid
            });

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), incomingRelationSystemUsage.Uuid, new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = systemUsage.Uuid
            });

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = outgoingRelationSystemUsage.Uuid,
                    RelationInterfaceUuid = relationInterface.Uuid
                });

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Get current system usage
            var updatedSystemUsage = await ItSystemUsageV2Helper.GetSingleAsync(await GetGlobalToken(), systemUsage.Uuid);

            //Act 
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            // From System Usage
            Assert.Equal(systemUsage.Uuid, readModel.SourceEntityUuid);
            Assert.Equal(organizationId, readModel.OrganizationId);
            Assert.Equal(systemUsageVersion, readModel.Version);
            Assert.Equal(systemUsageLocalCallName, readModel.LocalCallName);
            Assert.Equal(updatedSystemUsage.CreatedBy.Name, readModel.ObjectOwnerName);
            Assert.Equal(updatedSystemUsage.LastModifiedBy.Name, readModel.LastChangedByName);
            Assert.Equal(concluded.Date, readModel.Concluded?.Date);
            Assert.Equal(systemUsageExpirationDate.Date, readModel.ExpirationDate?.Date);
            Assert.True(readModel.ActiveAccordingToValidityPeriod);
            Assert.True(readModel.ActiveAccordingToLifeCycle);
            Assert.True(readModel.SystemActive);
            Assert.Equal(updatedSystemUsage.LastModified.Date, readModel.LastChangedAt.Date);
            Assert.Equal(archiveDuty.ToArchiveDutyTypes(), readModel.ArchiveDuty);
            Assert.Equal(isHoldingDocument, readModel.IsHoldingDocument);
            Assert.Equal(linkToDirectoryUrlName, readModel.LinkToDirectoryName);
            Assert.Equal(linkToDirectoryUrl, readModel.LinkToDirectoryUrl);
            Assert.Equal(generalPurpose, readModel.GeneralPurpose);
            Assert.Equal(hostedAt, readModel.HostedAt);
            Assert.Equal(userCount, readModel.UserCount);

            if (riskAssessment == YesNoDontKnowChoice.Yes)
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
            Assert.Equal(sensitiveDataLevel.ToSensitiveDataLevel(), rmSensitiveDataLevel.SensitivityDataLevel);
            Assert.Equal(sensitiveDataLevel.ToSensitiveDataLevel().GetReadableName(), readModel.SensitiveDataLevelsAsCsv);

            // From System
            Assert.Equal(systemName, readModel.SystemName);
            Assert.Equal(systemPreviousName, readModel.SystemPreviousName);
            Assert.Equal(systemDescription, readModel.SystemDescription);
            Assert.Equal(systemDisabled, readModel.ItSystemDisabled);
            Assert.Equal(system.Uuid.ToString("D"), readModel.ItSystemUuid);
            Assert.Equal(businessType.Uuid, readModel.ItSystemBusinessTypeUuid);
            Assert.Equal(businessType.Name, readModel.ItSystemBusinessTypeName);
            Assert.Equal(dataClassification.Uuid, readModel.ItSystemCategoriesUuid);
            Assert.Equal(dataClassification.Name, readModel.ItSystemCategoriesName);
            Assert.Equal(organizationId, readModel.ItSystemRightsHolderId);
            Assert.Equal(organizationName, readModel.ItSystemRightsHolderName);
            Assert.Equal(taskRef.Description, readModel.ItSystemKLENamesAsCsv);
            var readTaskRef = Assert.Single(readModel.ItSystemTaskRefs);
            Assert.Equal(taskRef.KleNumber, readTaskRef.KLEId);
            Assert.Equal(taskRef.Description, readTaskRef.KLEName);

            // From Parent System
            Assert.Equal(systemParentName, readModel.ParentItSystemName);
            Assert.Equal(systemParent.Uuid, readModel.ParentItSystemUuid);
            Assert.Equal(systemParentDisabled, readModel.ParentItSystemDisabled);
            Assert.Equal(systemParentUsage.Uuid, readModel.ParentItSystemUsageUuid);

            // Role assignment
            var roleAssignment = Assert.Single(readModel.RoleAssignments);

            Assert.Equal(role.Uuid, roleAssignment.RoleUuid);
            Assert.Equal(role.Uuid, roleAssignment.RoleUuid);
            Assert.Equal(DatabaseAccess.GetEntityId<User>(user.Uuid), roleAssignment.UserId); //Probably need UserUuid on roleassignments
            Assert.Equal($"{user.FirstName} {user.LastName}", roleAssignment.UserFullName);
            Assert.Equal(user.Email, roleAssignment.Email);

            // Responsible Organization Unit
            Assert.Equal(responsibleUnit.Uuid, readModel.ResponsibleOrganizationUnitUuid);
            Assert.Equal(responsibleUnit.Name, readModel.ResponsibleOrganizationUnitName);
            Assert.Contains(readModel.RelevantOrganizationUnits, orgUnitReadModel => MatchExpectedOrgUnit(orgUnitReadModel, organizationUnit1));
            Assert.Contains(readModel.RelevantOrganizationUnits, orgUnitReadModel => MatchExpectedOrgUnit(orgUnitReadModel, organizationUnit2));

            // Reference
            Assert.Equal(reference.Title, readModel.LocalReferenceTitle);
            Assert.Equal(reference.Url, readModel.LocalReferenceUrl);
            Assert.Equal(reference.DocumentId, readModel.LocalReferenceDocumentId);

            // Main Contract
            Assert.Equal(contract1.Uuid, DatabaseAccess.GetEntityUuid<ItContract>(readModel.MainContractId ?? 0)); //also MainContractUuid
            Assert.Equal(organizationId, readModel.MainContractSupplierId);
            Assert.Equal(organizationName, readModel.MainContractSupplierName);
            Assert.True(readModel.MainContractIsActive);

            // Associated contracts
            var expectedContracts = new[] { contract1, contract2 }.ToList();
            Assert.Equal(expectedContracts.Count, readModel.AssociatedContracts.Count);
            foreach (var expectedContract in expectedContracts)
            {
                Assert.Contains(expectedContract.Name, readModel.AssociatedContractsNamesCsv);
                Assert.Contains(readModel.AssociatedContracts, c => c.ItContractUuid == expectedContract.Uuid);
            }

            // ArchivePeriods
            var rmArchivePeriod = Assert.Single(readModel.ArchivePeriods);
            Assert.Equal(archivePeriodStartDate.Date, rmArchivePeriod.StartDate.Date);
            Assert.Equal(archivePeriodEndDate.Date, rmArchivePeriod.EndDate.Date);
            Assert.Equal(archivePeriodEndDate.Date, readModel.ActiveArchivePeriodEndDate?.Date);

            // DataProcessingRegistration
            Assert.Equal(dataProcessingRegistration.Name, readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal(yesNoIrrelevantOption.ToYesNoIrrelevantOption().GetReadableName(), readModel.DataProcessingRegistrationsConcludedAsCsv);
            var dpr = Assert.Single(readModel.DataProcessingRegistrations);
            Assert.Equal(dataProcessingRegistration.Uuid, dpr.DataProcessingRegistrationUuid);

            // DependsOnInterfaces 
            Assert.Equal(relationInterfaceName, readModel.DependsOnInterfacesNamesAsCsv);
            var rmDependsOnInterface = Assert.Single(readModel.DependsOnInterfaces);
            Assert.Equal(relationInterface.Uuid, rmDependsOnInterface.InterfaceUuid);
            Assert.Equal(relationInterfaceName, rmDependsOnInterface.InterfaceName);

            //Incoming system usages
            Assert.Equal(incomingRelationSystemName, readModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            var rmIncomingRelatedItSystemUsage = Assert.Single(readModel.IncomingRelatedItSystemUsages);
            Assert.Equal(incomingRelationSystemName, rmIncomingRelatedItSystemUsage.ItSystemUsageName);

            //Outgoing system usages
            Assert.Equal(outgoingRelationSystemName, readModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
            var rmOutgoingRelatedItSystemUsage = Assert.Single(readModel.OutgoingRelatedItSystemUsages);
            Assert.Equal(outgoingRelationSystemUsage.Uuid, rmOutgoingRelatedItSystemUsage.ItSystemUsageUuid);
            Assert.Equal(outgoingRelationSystemName, rmOutgoingRelatedItSystemUsage.ItSystemUsageName);
        }

        [Fact]
        public async Task ReadModels_ItSystemParentName_Is_Null_When_No_Parent()
        {
            //Arrange
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var organizationUuid = DatabaseAccess.GetEntityUuid<Organization>(organizationId);

            var system = await CreateItSystemAsync(organizationUuid, name: systemName);
            await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

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
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, name: systemName);
            var systemParent = await CreateItSystemAsync(organizationUuid, name: systemParentName);
            await ItSystemV2Helper.PatchSystemAsync(await GetGlobalToken(), system.Uuid, new KeyValuePair<string, object>(nameof(UpdateItSystemRequestDTO.ParentUuid), systemParent.Uuid));
            await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemV2Helper.SendPatchSystemNameAsync(await GetGlobalToken(), systemParent.Uuid,
                newSystemParentName).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newSystemParentName, readModel.ParentItSystemName);
            Assert.Equal(systemParent.Uuid, readModel.ParentItSystemUuid);
        }

        [Fact]
        public async Task ReadModels_ResponsibleOrganizationUnit_Is_Updated_When_ResponsibleOrganizationUnit_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, name: systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var orgUnitName1 = A<string>();
            var orgUnitName2 = A<string>();
            var organizationUnit1 = await CreateOrganizationUnitAsync(organizationUuid, orgUnitName1);
            var organizationUnit2 = await CreateOrganizationUnitAsync(organizationUuid, orgUnitName2);

            await ItSystemUsageV2Helper.SendPatchOrganizationalUsage(await GetGlobalToken(), systemUsage.Uuid,
                new OrganizationUsageWriteRequestDTO
                {
                    UsingOrganizationUnitUuids = new[] { organizationUnit1, organizationUnit2 }.Select(x => x.Uuid),
                    ResponsibleOrganizationUnitUuid = organizationUnit1.Uuid
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemUsageV2Helper.SendPatchOrganizationalUsage(await GetGlobalToken(), systemUsage.Uuid,
                new OrganizationUsageWriteRequestDTO
                {
                    UsingOrganizationUnitUuids = new[] { organizationUnit1, organizationUnit2 }.Select(x => x.Uuid),
                    ResponsibleOrganizationUnitUuid = organizationUnit2.Uuid
                }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(organizationUnit2.Uuid, readModel.ResponsibleOrganizationUnitUuid);
            Assert.Equal(orgUnitName2, readModel.ResponsibleOrganizationUnitName);
        }

        [Fact]
        public async Task ReadModels_ItSystemRightsHolderName_Is_Updated_When_OrganizationName_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var organizationName1 = A<string>();
            var organizationName2 = A<string>();
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, name: systemName);
            await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var organization1 = await CreateOrganizationAsync(organizationName1);

            await ItSystemV2Helper.SendPatchRightsHolderAsync(await GetGlobalToken(), system.Uuid, organization1.Uuid).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await OrganizationInternalV2Helper.PatchOrganization(organization1.Uuid,
                new OrganizationUpdateRequestDTO { Name = organizationName2, Type = OrganizationType.Municipality }).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

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
            var newBusinessTypeName = A<string>();
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(DefaultOrgUuid, systemName);
            await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var businessType = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.BusinessType, organizationUuid);

            await ItSystemV2Helper.SendPatchBusinessTypeAsync(await GetGlobalToken(), system.Uuid, businessType.Uuid);

            //Wait for read model to rebuild (wait for the LAST mutation)k
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await GlobalOptionTypeV2Helper.PatchGlobalOptionType(businessType.Uuid,
                GlobalOptionTypeV2Helper.BusinessTypes, new GlobalRegularOptionUpdateRequestDTO
                {
                    Name = newBusinessTypeName,
                    IsEnabled = true,
                    IsObligatory = true
                });
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newBusinessTypeName, readModel.ItSystemBusinessTypeName);
        }

        [Fact]
        public async Task ReadModels_MainContractName_And_MainContractSupplierName_Is_Updated_When_MainContract_Is_Deleted()
        {
            //Arrange
            var systemName = A<string>();
            var contractName = A<string>();
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, name: systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var contract = await CreateItContractAsync(organizationUuid, contractName);

            await ItContractV2Helper.SendPatchContractSupplierAsync(await GetGlobalToken(),
                contract.Uuid, new ContractSupplierDataWriteRequestDTO { OrganizationUuid = organizationUuid });
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract.Uuid,
                systemUsage.Uuid.WrapAsEnumerable());

            await ItSystemUsageV2Helper.SendPatchGeneral(await GetGlobalToken(), systemUsage.Uuid,
                new GeneralDataUpdateRequestDTO { MainContractUuid = contract.Uuid });

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItContractV2Helper.DeleteContractAsync(await GetGlobalToken(), contract.Uuid);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

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
            var organizationUuid = DefaultOrgUuid;
            var yesNoIrrelevantOption = A<YesNoIrrelevantChoice>();

            var system = await CreateItSystemAsync(organizationUuid, systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var dataProcessingRegistration = await CreateDPRAsync(organizationUuid, dataProcessingRegistrationName);
            await DataProcessingRegistrationV2Helper.PatchIsAgreementConcludedAsync(dataProcessingRegistration.Uuid, A<YesNoIrrelevantChoice>());
            await DataProcessingRegistrationV2Helper.PatchSystemsAsync(dataProcessingRegistration.Uuid, systemUsage.Uuid.WrapAsEnumerable()).DisposeAsync();

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await DataProcessingRegistrationV2Helper.SendPatchName(await GetGlobalToken(),
                dataProcessingRegistration.Uuid, newDataProcessingRegistrationName).DisposeAsync();
            await DataProcessingRegistrationV2Helper.PatchIsAgreementConcludedAsync(dataProcessingRegistration.Uuid, yesNoIrrelevantOption);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newDataProcessingRegistrationName, readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal(yesNoIrrelevantOption.ToYesNoIrrelevantOption().GetReadableName(), readModel.DataProcessingRegistrationsConcludedAsCsv);
            var rmDataProcessingRegistration = Assert.Single(readModel.DataProcessingRegistrations);
            Assert.Equal(dataProcessingRegistration.Uuid, rmDataProcessingRegistration.DataProcessingRegistrationUuid);
            Assert.Equal(newDataProcessingRegistrationName, rmDataProcessingRegistration.DataProcessingRegistrationName);
            Assert.Equal(yesNoIrrelevantOption.ToYesNoIrrelevantOption(), rmDataProcessingRegistration.IsAgreementConcluded);
        }

        [Fact]
        public async Task ReadModels_DependsOnInterfacesNamesAsCsv_Is_Updated_When_Interface_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var relationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var newRelationInterfaceName = A<string>();
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var relationSystem = await CreateItSystemAsync(organizationUuid, relationSystemName);
            var relationSystemUsage = await TakeSystemIntoUsageAsync(relationSystem.Uuid, organizationUuid);

            var relationInterface = await CreateItInterfaceAsync(organizationUuid, relationInterfaceName);

            await InterfaceV2Helper.SendPatchExposedBySystemAsync(await GetGlobalToken(), relationInterface.Uuid,
                relationSystem.Uuid);

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = relationSystemUsage.Uuid,
                    RelationInterfaceUuid = relationInterface.Uuid
                });


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await InterfaceV2Helper.SendPatchInterfaceAsync(await GetGlobalToken(), relationInterface.Uuid, x => x.Name,
                newRelationInterfaceName);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");


            //Assert
            var mainSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newRelationInterfaceName, mainSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.IncomingRelatedItSystemUsages);
            var rmInterface = Assert.Single(mainSystemReadModel.DependsOnInterfaces);
            Assert.Equal(relationInterface.Uuid, rmInterface.InterfaceUuid);
            Assert.Equal(newRelationInterfaceName, rmInterface.InterfaceName);


            var relationSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, relationSystemName, 1, 0)).ToList();
            var relationSystemReadModel = Assert.Single(relationSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(systemName, relationSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.DependsOnInterfaces);
            var rmIncomingSystemUsage = Assert.Single(relationSystemReadModel.IncomingRelatedItSystemUsages);
            Assert.Equal(systemUsage.Uuid, rmIncomingSystemUsage.ItSystemUsageUuid);
            Assert.Equal(systemName, rmIncomingSystemUsage.ItSystemUsageName);
        }

        [Fact]
        public async Task ReadModels_DependsOnInterfacesNamesAsCsv_Is_Updated_When_SystemRelation_Is_Deleted()
        {
            //Arrange
            var systemName = A<string>();
            var relationSystemName = A<string>();
            var relationInterfaceName = A<string>();
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var relationSystem = await CreateItSystemAsync(organizationUuid, relationSystemName);
            var relationSystemUsage = await TakeSystemIntoUsageAsync(relationSystem.Uuid, organizationUuid);
            var relationInterface = await CreateItInterfaceAsync(organizationUuid, relationInterfaceName);
            await InterfaceV2Helper.SendPatchExposedBySystemAsync(await GetGlobalToken(), relationInterface.Uuid, relationSystem.Uuid);

            var relation = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = relationSystemUsage.Uuid,
                    RelationInterfaceUuid = relationInterface.Uuid
                });


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemUsageV2Helper.SendDeleteRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                relation.Uuid);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Assert
            var mainSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal("", mainSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.DependsOnInterfaces);
            Assert.Equal("", mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.IncomingRelatedItSystemUsages);


            var relationSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, relationSystemName, 1, 0)).ToList();
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
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var relationSystem = await CreateItSystemAsync(organizationUuid, relationSystemName);
            var relationSystemUsage = await TakeSystemIntoUsageAsync(relationSystem.Uuid, organizationUuid);

            var newRelationSystem = await CreateItSystemAsync(organizationUuid, newRelationSystemName);
            var newRelationSystemUsage = await TakeSystemIntoUsageAsync(newRelationSystem.Uuid, organizationUuid);

            var relationInterface = await CreateItInterfaceAsync(organizationUuid, relationInterfaceName);
            await InterfaceV2Helper.SendPatchExposedBySystemAsync(await GetGlobalToken(), relationInterface.Uuid, relationSystem.Uuid).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var relation = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = relationSystemUsage.Uuid,
                    RelationInterfaceUuid = relationInterface.Uuid
                });


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemUsageV2Helper.PutRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                relation.Uuid, new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = newRelationSystemUsage.Uuid,
                    RelationInterfaceUuid = null,
                });

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Assert
            var mainSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newRelationSystemName, mainSystemReadModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
            Assert.Equal("", mainSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.DependsOnInterfaces);
            Assert.Equal("", mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(mainSystemReadModel.IncomingRelatedItSystemUsages);


            var relationSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, relationSystemName, 1, 0)).ToList();
            var relationSystemReadModel = Assert.Single(relationSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal("", relationSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.IncomingRelatedItSystemUsages);
            Assert.Equal("", relationSystemReadModel.DependsOnInterfacesNamesAsCsv);
            Assert.Empty(relationSystemReadModel.DependsOnInterfaces);


            var newRelationSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, newRelationSystemName, 1, 0)).ToList();
            var newRelationSystemReadModel = Assert.Single(newRelationSystemReadModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(systemName, newRelationSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            var rmSystemUsage = Assert.Single(newRelationSystemReadModel.IncomingRelatedItSystemUsages);
            Assert.Equal(systemUsage.Uuid, rmSystemUsage.ItSystemUsageUuid);
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
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var relationSystem = await CreateItSystemAsync(organizationUuid, relationSystemName);
            var relationSystemUsage = await TakeSystemIntoUsageAsync(relationSystem.Uuid, organizationUuid);

            var relationInterface = await CreateItInterfaceAsync(organizationUuid, relationInterfaceName);
            await InterfaceV2Helper.SendPatchExposedBySystemAsync(await GetGlobalToken(), relationInterface.Uuid, relationSystem.Uuid);

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                { ToSystemUsageUuid = relationSystemUsage.Uuid, RelationInterfaceUuid = relationInterface.Uuid });
            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), relationSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = systemUsage.Uuid
                });

            //Await first update
            await WaitForReadModelQueueDepletion();

            //Act - Second update should blank out the relation fields since the affected usage has been killed
            using var removeUsage = await ItSystemUsageV2Helper.SendDeleteAsync(await GetGlobalToken(), relationSystemUsage.Uuid);
            Assert.Equal(HttpStatusCode.NoContent, removeUsage.StatusCode);

            //Assert
            await WaitForReadModelQueueDepletion();
            var mainSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();
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
            var outgoingRelationSystemNameInitial = A<string>();
            var outgoingRelationSystemNameChanged = $"{outgoingRelationSystemNameInitial}_1";
            var incomingRelationSystemNameInitial = A<string>();
            var incomingRelationSystemNameChanged = $"{incomingRelationSystemNameInitial}_1";
            var organizationUuid = DefaultOrgUuid;

            var system = await CreateItSystemAsync(organizationUuid, systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            var outGoingRelationSystem =
                await CreateItSystemAsync(organizationUuid, outgoingRelationSystemNameInitial);
            var outGoingRelationSystemUsage =
                await TakeSystemIntoUsageAsync(outGoingRelationSystem.Uuid, organizationUuid);

            var incomingRelationSystem =
                await CreateItSystemAsync(organizationUuid, incomingRelationSystemNameInitial);
            var incomingRelationSystemUsage = await TakeSystemIntoUsageAsync(incomingRelationSystem.Uuid, organizationUuid);

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), systemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = outGoingRelationSystemUsage.Uuid
                });
            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), incomingRelationSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = systemUsage.Uuid
                });

            //Await first update
            await WaitForReadModelQueueDepletion();

            //Act + assert - Rename the system used in incoming relation and verify that the readmodel is updated
            using var renameIncomingSystem = await ItSystemV2Helper.SendPatchSystemNameAsync(await GetGlobalToken(),
                incomingRelationSystem.Uuid, incomingRelationSystemNameChanged);
            Assert.Equal(HttpStatusCode.OK, renameIncomingSystem.StatusCode);

            await WaitForReadModelQueueDepletion();
            var mainSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();
            var mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Assert.Equal(incomingRelationSystemNameChanged, mainSystemReadModel.IncomingRelatedItSystemUsagesNamesAsCsv);

            //Act + assert - Rename the system used in outgoing relation and verify that the readmodel is updated
            using var renameOutgoingSystem = await ItSystemV2Helper.SendPatchSystemNameAsync(await GetGlobalToken(),
                outGoingRelationSystem.Uuid, outgoingRelationSystemNameChanged);
            Assert.Equal(HttpStatusCode.OK, renameOutgoingSystem.StatusCode);
            await WaitForReadModelQueueDepletion();

            mainSystemReadModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();
            mainSystemReadModel = Assert.Single(mainSystemReadModels);
            Assert.Equal(outgoingRelationSystemNameChanged, mainSystemReadModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
        }

        [Fact]
        public async Task When_SystemRightIsDeleted_Role_Assignment_In_Readmodel_Is_Updated()
        {
            //Arrange
            var organizationUuid = DefaultOrgUuid;

            var systemName = A<string>();
            var system = await CreateItSystemAsync(organizationUuid, systemName);
            var systemUsage = await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);

            // Role assignment
            var role = await OptionV2ApiHelper.GetRandomOptionAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRoles,
                organizationUuid);
            var availableUsers = await OrganizationV2Helper.GetUsersInOrganization(organizationUuid);
            var user = availableUsers.First();
            using var assignRoleResponse = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(
                await GetGlobalToken(), systemUsage.Uuid, new RoleAssignmentRequestDTO
                {
                    UserUuid = user.Uuid,
                    RoleUuid = role.Uuid
                });
            Assert.Equal(HttpStatusCode.OK, assignRoleResponse.StatusCode);


            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            await Console.Out.WriteLineAsync("Read models are up to date");

            //Act 
            var readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            await Console.Out.WriteLineAsync("Read model found");
            Assert.Single(readModel.RoleAssignments);
            await Console.Out.WriteLineAsync("Found one role assignment as expected");

            //Act - remove the right using the odata api
            var rightUuid = DatabaseAccess.MapFromEntitySet<ItSystemRight, Guid>(rights => rights.AsQueryable().Single(x => x.ObjectId == readModel.SourceEntityId).Role.Uuid);
            await LocalOptionTypeV2Helper.DeleteLocalOptionType(organizationUuid, rightUuid, "it-systems-roles",
                "api/v2/internal/it-systems").WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            await WaitForReadModelQueueDepletion();
            await Console.Out.WriteLineAsync("Read models are up to date");

            readModels = (await ItSystemUsageV2Helper.QueryReadModelByNameContent(organizationUuid, systemName, 1, 0)).ToList();
            readModel = Assert.Single(readModels);
            //Assert.Empty(readModel.RoleAssignments); todo: New endpoints don't seem to have this effect?
        }

        private static async Task WaitForReadModelQueueDepletion()
        {
            await ReadModelTestTools.WaitForReadModelQueueDepletion();
        }

        private static bool MatchExpectedOrgUnit(ItSystemUsageOverviewRelevantOrgUnitReadModel x, OrganizationUnitResponseDTO organizationUnit1)
        {
            return x.OrganizationUnitUuid == organizationUnit1.Uuid && x.OrganizationUnitName == organizationUnit1.Name;
        }

        private async Task<ItSystemResponseDTO> PrepareItSystem(string systemName, string systemPreviousName, string systemDescription, int organizationId, string organizationName)
        {
            var organization = await CreateOrganizationAsync(organizationName, TestCvr, OrganizationType.Company);
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

        private static ExpectedUsersIntervalDTO UserIntervalDtoFromUerCount(UserCount count)
        {
            int lower = 0;
            int? upper = null;

            switch (count)
            {
                case UserCount.BELOWTEN:
                    lower = 0;
                    upper = 9;
                    break;
                case UserCount.TENTOFIFTY:
                    lower = 10;
                    upper = 50;
                    break;
                case UserCount.FIFTYTOHUNDRED:
                    lower = 50;
                    upper = 100;
                    break;
                case UserCount.HUNDREDPLUS:
                    lower = 100;
                    break;
                case UserCount.UNDECIDED:
                    return null;
            }
            return new ExpectedUsersIntervalDTO { LowerBound = lower, UpperBound = upper };
        }
    }
}
