using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Options;
using Core.DomainServices.SystemUsage;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdateTest : WithAutoFixture
    {

        private readonly Mock<IOptionsService<ItSystem, BusinessType>> _businessTypeService;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel>> _roleAssignmentRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewTaskRefReadModel>> _taskRefRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewSensitiveDataLevelReadModel>> _sensitiveDataLevelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel>> _archivePeriodReadModelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel>> _dataProcessingReadModelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewInterfaceReadModel>> _interfacesReadModelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewUsedBySystemUsageReadModel>> _itSystemUsageReadModelRepository;
        private readonly ItSystemUsageOverviewReadModelUpdate _sut;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewRelevantOrgUnitReadModel>> _orgUnitRepoMock;

        public ItSystemUsageOverviewReadModelUpdateTest()
        {
            _businessTypeService = new Mock<IOptionsService<ItSystem, BusinessType>>();
            _taskRefRepository = new Mock<IGenericRepository<ItSystemUsageOverviewTaskRefReadModel>>();
            _sensitiveDataLevelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewSensitiveDataLevelReadModel>>();
            _roleAssignmentRepository = new Mock<IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel>>();
            _archivePeriodReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel>>();
            _dataProcessingReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel>>();
            _interfacesReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewInterfaceReadModel>>();
            _itSystemUsageReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewUsedBySystemUsageReadModel>>();
            _orgUnitRepoMock = new Mock<IGenericRepository<ItSystemUsageOverviewRelevantOrgUnitReadModel>>();
            _itContractReadModelRepoMock = new Mock<IGenericRepository<ItSystemUsageOverviewItContractReadModel>>();
            _sut = new ItSystemUsageOverviewReadModelUpdate(
                _roleAssignmentRepository.Object,
                _taskRefRepository.Object,
                _sensitiveDataLevelRepository.Object,
                _archivePeriodReadModelRepository.Object,
                _dataProcessingReadModelRepository.Object,
                _interfacesReadModelRepository.Object,
                _itSystemUsageReadModelRepository.Object,
                Mock.Of<IGenericRepository<ItSystemUsageOverviewUsingSystemUsageReadModel>>(),
                _businessTypeService.Object,
                _orgUnitRepoMock.Object,
                _itContractReadModelRepoMock.Object);
        }

        private static readonly User DefaultTestUser = new()
        {
            Id = 1,
            Name = "test",
            LastName = "tester",
            Email = $"test@tester.dk"
        };

        private readonly Mock<IGenericRepository<ItSystemUsageOverviewItContractReadModel>> _itContractReadModelRepoMock;

        [Fact]
        public void Apply_Generates_Correct_Read_Model()
        {
            //Arrange
            var outgoingRelationItSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>(),
                ExternalUuid = A<Guid>()
            };
            var outgoingRelationInterface = new ItInterface
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var outgoingRelationItSystemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = outgoingRelationItSystem
            };
            var outgoingRelation = new SystemRelation(outgoingRelationItSystemUsage)
            {
                Id = A<int>(),
                RelationInterface = outgoingRelationInterface,
                ToSystemUsage = outgoingRelationItSystemUsage
            };

            var incomingRelationItSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var incomingRelationItSystemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = incomingRelationItSystem
            };
            var incomingRelation = new SystemRelation(incomingRelationItSystemUsage)
            {
                Id = A<int>()
            };

            var supplier = new Organization
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var user = new User
            {
                Id = A<int>(),
                Name = A<string>(),
                LastName = A<string>(),
                Email = $"{A<string>()}@{A<string>()}.dk"
            };
            var right = new ItSystemRight
            {
                Id = A<int>(),
                User = user,
                UserId = user.Id,
                Role = new()
                {
                    Id = A<int>()
                }
            };
            var contract1 = new ItContract
            {
                Id = A<int>(),
                Name = A<string>(),
                Supplier = supplier
            };
            var contract2 = new ItContract
            {
                Id = A<int>(),
                Name = A<string>(),
                Supplier = supplier
            };
            var dataProcessingRegistration = new DataProcessingRegistration()
            {
                Id = A<int>(),
                Name = A<string>(),
                IsAgreementConcluded = A<YesNoIrrelevantOption>()
            };
            var organizationId = A<int>();
            var parentItSystemUsage = new ItSystemUsage()
            {
                Uuid = A<Guid>(),
                OrganizationId = organizationId
            };
            var parentSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>(),
                Usages = new List<ItSystemUsage>(){ parentItSystemUsage },
            };
            var system = new ItSystem
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                Name = A<string>(),
                Description = A<string>(),
                Disabled = A<bool>(),
                PreviousName= A<string>(),
                Parent = parentSystem,
                Uuid = A<Guid>(),
                BelongsTo = new Organization
                {
                    Id = A<int>(),
                    Name = A<string>()
                },
                BusinessType = new BusinessType
                {
                    Id = A<int>(),
                    Name = A<string>()
                },
                TaskRefs = new List<TaskRef>
                {
                    new TaskRef
                    {
                        Id = A<int>(),
                        TaskKey = A<string>(),
                        Description = A<string>()
                    }
                }
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = organizationId,
                ItSystem = system,
                ExpirationDate = DateTime.Now.AddDays(-1),
                Version = A<string>(),
                LocalCallName = A<string>(),
                LocalSystemId = A<string>(),
                Rights = new List<ItSystemRight>
                {
                    right
                },
                Reference = new ExternalReference
                {
                    Id = A<int>(),
                    Title = A<string>(),
                    ExternalReferenceId = A<string>(),
                    URL = A<string>()
                },
                ObjectOwnerId = user.Id,
                ObjectOwner = user,
                LastChangedByUserId = user.Id,
                LastChangedByUser = user,
                LastChanged = A<DateTime>(),
                Concluded = A<DateTime>(),
                ArchiveDuty = A<ArchiveDutyTypes>(),
                Registertype = A<bool>(),
                riskAssessment = DataOptions.YES,
                riskAssesmentDate = A<DateTime>(),
                RiskSupervisionDocumentationUrlName = A<string>(),
                RiskSupervisionDocumentationUrl = A<string>(),
                PlannedRiskAssessmentDate = A<DateTime>(),
                LinkToDirectoryUrlName = A<string>(),
                LinkToDirectoryUrl = A<string>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>
                {
                    dataProcessingRegistration
                },
                GeneralPurpose = A<string>(),
                HostedAt = A<HostedAt>(),
                UserCount = A<UserCount>(),
                UsageRelations = new List<SystemRelation>
                {
                    outgoingRelation
                },
                UsedByRelations = new List<SystemRelation>
                {
                    incomingRelation
                },
                LifeCycleStatus = A<LifeCycleStatusType>()
            };

            // Add ResponsibleOrganizationUnit
            var responsibleOrgUnitUsage = CreateOrganizationUnitUsage(systemUsage);
            systemUsage.ResponsibleUsage = responsibleOrgUnitUsage;

            // Relevant org unit usages
            var relevantUsages = Enumerable.Range(0, 2).Select(_ => CreateOrganizationUnitUsage(systemUsage)).ToList();
            relevantUsages.ForEach(systemUsage.UsedBy.Add);

            _businessTypeService.Setup(x => x.GetOption(system.OrganizationId, system.BusinessType.Id)).Returns(Maybe<(BusinessType, bool)>.Some((system.BusinessType, true)));

            // Add MainContract
            var mainContract = AssociateContract(contract1, systemUsage);
            systemUsage.MainContract = mainContract;

            //Add contracts
            systemUsage.Contracts.Add(mainContract);
            systemUsage.Contracts.Add(AssociateContract(contract2, systemUsage));

            // Add SensitiveDataLevel
            var sensitiveDataLevel = new ItSystemUsageSensitiveDataLevel
            {
                Id = A<int>(),
                ItSystemUsage = systemUsage,
                SensitivityDataLevel = A<SensitiveDataLevel>()
            };
            systemUsage.SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel> { sensitiveDataLevel };

            // Add ArchivePeriod
            var archivePeriods = new List<ArchivePeriod>
            {
                new()
                {
                    Id = A<int>(),
                    ItSystemUsage = systemUsage,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(1)
                }
            };
            systemUsage.ArchivePeriods = archivePeriods;

            systemUsage.ItSystemCategories = new ItSystemCategories
                { Id = A<int>(), Uuid = A<Guid>(), Name = A<string>() };

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            //System usage
            Assert.Equal(systemUsage.Id, readModel.SourceEntityId);
            Assert.Equal(systemUsage.Uuid, readModel.SourceEntityUuid);
            Assert.Equal(systemUsage.OrganizationId, readModel.OrganizationId);
            Assert.Equal(systemUsage.IsActiveAccordingToDateFields, readModel.ActiveAccordingToValidityPeriod);
            Assert.Equal(systemUsage.IsActiveAccordingToLifeCycle, readModel.ActiveAccordingToLifeCycle);
            Assert.Equal(systemUsage.CheckSystemValidity().Result, readModel.SystemActive);
            Assert.Equal(systemUsage.Version, readModel.Version);
            Assert.Equal(systemUsage.LocalCallName, readModel.LocalCallName);
            Assert.Equal(systemUsage.LocalSystemId, readModel.LocalSystemId);
            Assert.Equal(user.Id, readModel.ObjectOwnerId);
            Assert.Equal(user.GetFullName(), readModel.ObjectOwnerName);
            Assert.Equal(user.Id, readModel.LastChangedById);
            Assert.Equal(user.GetFullName(), readModel.LastChangedByName);
            Assert.Equal(systemUsage.LastChanged.Date, readModel.LastChangedAt.Date);
            Assert.Equal(systemUsage.LifeCycleStatus, readModel.LifeCycleStatus);
            Assert.Equal(systemUsage.Concluded?.Date, readModel.Concluded?.Date);
            Assert.Equal(systemUsage.ExpirationDate?.Date, readModel.ExpirationDate?.Date);
            Assert.Equal(systemUsage.ArchiveDuty, readModel.ArchiveDuty);
            Assert.Equal(systemUsage.Registertype, readModel.IsHoldingDocument);
            Assert.Equal(systemUsage.RiskSupervisionDocumentationUrlName, readModel.RiskSupervisionDocumentationName);
            Assert.Equal(systemUsage.RiskSupervisionDocumentationUrl, readModel.RiskSupervisionDocumentationUrl);
            Assert.Equal(systemUsage.LinkToDirectoryUrlName, readModel.LinkToDirectoryName);
            Assert.Equal(systemUsage.LinkToDirectoryUrl, readModel.LinkToDirectoryUrl);
            Assert.Equal(systemUsage.GeneralPurpose, readModel.GeneralPurpose);
            Assert.Equal(systemUsage.HostedAt, readModel.HostedAt);
            Assert.Equal(systemUsage.UserCount, readModel.UserCount);
            Assert.Equal(systemUsage.riskAssesmentDate, readModel.RiskAssessmentDate);
            Assert.Equal(systemUsage.PlannedRiskAssessmentDate, readModel.PlannedRiskAssessmentDate);
            Assert.Equal(systemUsage.ItSystem.PreviousName, readModel.SystemPreviousName);
            Assert.Equal(systemUsage.ItSystem.Description, readModel.SystemDescription);

            // Sensitive data levels
            var rmSensitiveDataLevel = Assert.Single(readModel.SensitiveDataLevels);
            Assert.Equal(sensitiveDataLevel.SensitivityDataLevel, rmSensitiveDataLevel.SensitivityDataLevel);
            Assert.Equal(sensitiveDataLevel.SensitivityDataLevel.GetReadableName(), readModel.SensitiveDataLevelsAsCsv);

            //System
            Assert.Equal(system.Name, readModel.SystemName);
            Assert.Equal(system.Disabled, readModel.ItSystemDisabled);
            Assert.Equal(system.Uuid.ToString("D"), readModel.ItSystemUuid);
            Assert.Equal(system.ExternalUuid, readModel.ExternalSystemUuid);
            Assert.Equal(system.BelongsTo.Id, readModel.ItSystemRightsHolderId);
            Assert.Equal(system.BelongsTo.Name, readModel.ItSystemRightsHolderName);
            Assert.Equal(systemUsage.ItSystemCategories.Uuid, readModel.ItSystemCategoriesUuid);
            Assert.Equal(systemUsage.ItSystemCategories.Id, readModel.ItSystemCategoriesId);
            Assert.Equal(systemUsage.ItSystemCategories.Name, readModel.ItSystemCategoriesName);
            Assert.Equal(system.BusinessType.Id, readModel.ItSystemBusinessTypeId);
            Assert.Equal(system.BusinessType.Uuid, readModel.ItSystemBusinessTypeUuid);
            Assert.Equal(system.BusinessType.Name, readModel.ItSystemBusinessTypeName);

            //Parent System
            Assert.Equal(parentSystem.Name, readModel.ParentItSystemName);
            Assert.Equal(parentSystem.Id, readModel.ParentItSystemId);
            Assert.Equal(parentSystem.Disabled, readModel.ParentItSystemDisabled);
            Assert.Equal(system.Parent.Usages.FirstOrDefault()!.Uuid, readModel.ParentItSystemUsageUuid);

            //Assigned Roles
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.GetFullName(), roleAssignment.UserFullName);
            Assert.Equal(right.Role.Id, roleAssignment.RoleId);
            Assert.Equal(right.Role.Uuid, roleAssignment.RoleUuid);
            Assert.Equal(user.Email, roleAssignment.Email);

            //Responsible Organization Unit
            AssertOrgUnitUsage(responsibleOrgUnitUsage, readModel);

            //Relevant org unit usages
            Assert.Equal(relevantUsages.Count, readModel.RelevantOrganizationUnits.Count);
            var expectedAndActual = relevantUsages
                .OrderBy(x => x.OrganizationUnit.Uuid)
                .Zip
                (
                    readModel.RelevantOrganizationUnits.OrderBy(x => x.OrganizationUnitUuid),
                    (expected, actual) => new { expected, actual }
                )
                .ToList();
            expectedAndActual.ForEach(x => AssertOrgUnitUsage(x.expected, x.actual));

            //KLE
            Assert.Equal(system.TaskRefs.First().TaskKey, readModel.ItSystemKLEIdsAsCsv);
            Assert.Equal(system.TaskRefs.First().Description, readModel.ItSystemKLENamesAsCsv);
            var taskRef = Assert.Single(readModel.ItSystemTaskRefs);
            Assert.Equal(system.TaskRefs.First().TaskKey, taskRef.KLEId);
            Assert.Equal(system.TaskRefs.First().Description, taskRef.KLEName);

            //Reference
            Assert.Equal(systemUsage.Reference.Title, readModel.LocalReferenceTitle);
            Assert.Equal(systemUsage.Reference.URL, readModel.LocalReferenceUrl);
            Assert.Equal(systemUsage.Reference.ExternalReferenceId, readModel.LocalReferenceDocumentId);

            //Main Contract
            Assert.Equal(contract1.Id, readModel.MainContractId);
            Assert.Equal(contract1.Supplier.Id, readModel.MainContractSupplierId);
            Assert.Equal(contract1.Supplier.Name, readModel.MainContractSupplierName);
            Assert.Equal(contract1.IsActive, readModel.MainContractIsActive);

            //AsociatedContracts
            var expectedContracts = new[] { contract1, contract2 }.ToList();
            Assert.Equal(expectedContracts.Count, readModel.AssociatedContracts.Count);
            expectedContracts.ForEach(contract =>
            {
                Assert.Contains(contract.Name, readModel.AssociatedContractsNamesCsv);
                Assert.Contains(readModel.AssociatedContracts, associatedContract =>
                    associatedContract.ItContractId == contract.Id &&
                    associatedContract.ItContractName == contract.Name &&
                    associatedContract.ItContractUuid == contract.Uuid);
            });

            //ArchivePeriods
            var rmArchivePeriod = Assert.Single(readModel.ArchivePeriods);
            Assert.Equal(archivePeriods.First().StartDate, rmArchivePeriod.StartDate);
            Assert.Equal(archivePeriods.First().EndDate, rmArchivePeriod.EndDate);
            Assert.Equal(archivePeriods.First().EndDate, readModel.ActiveArchivePeriodEndDate);

            //DataProcessingRegistrations
            Assert.Equal(dataProcessingRegistration.Name, readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal(dataProcessingRegistration.IsAgreementConcluded.GetValueOrDefault(YesNoIrrelevantOption.UNDECIDED).GetReadableName(), readModel.DataProcessingRegistrationsConcludedAsCsv);
            var rmDataProcessingRegistration = Assert.Single(readModel.DataProcessingRegistrations);
            Assert.Equal(dataProcessingRegistration.Uuid, rmDataProcessingRegistration.DataProcessingRegistrationUuid);
            Assert.Equal(dataProcessingRegistration.Name, rmDataProcessingRegistration.DataProcessingRegistrationName);
            Assert.Equal(dataProcessingRegistration.IsAgreementConcluded, rmDataProcessingRegistration.IsAgreementConcluded);

            //Outgoing Relation interfaces
            Assert.Equal(outgoingRelationInterface.Name, readModel.DependsOnInterfacesNamesAsCsv);
            var rmDependsOnInterface = Assert.Single(readModel.DependsOnInterfaces);
            Assert.Equal(outgoingRelationInterface.Id, rmDependsOnInterface.InterfaceId);
            Assert.Equal(outgoingRelationInterface.Uuid, rmDependsOnInterface.InterfaceUuid);
            Assert.Equal(outgoingRelationInterface.Name, rmDependsOnInterface.InterfaceName);

            //Outgoing Relation systems
            Assert.Equal(outgoingRelationItSystem.Name, readModel.OutgoingRelatedItSystemUsagesNamesAsCsv);
            var rmDependsOnSystem = Assert.Single(readModel.OutgoingRelatedItSystemUsages);
            Assert.Equal(outgoingRelationItSystemUsage.Id, rmDependsOnSystem.ItSystemUsageId);
            Assert.Equal(outgoingRelationItSystemUsage.Uuid, rmDependsOnSystem.ItSystemUsageUuid);
            Assert.Equal(outgoingRelationItSystem.Name, rmDependsOnSystem.ItSystemUsageName);

            //Incoming Relations
            Assert.Equal(incomingRelationItSystem.Name, readModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            var rmIncomingRelatedSystemUsage = Assert.Single(readModel.IncomingRelatedItSystemUsages);
            Assert.Equal(incomingRelationItSystemUsage.Id, rmIncomingRelatedSystemUsage.ItSystemUsageId);
            Assert.Equal(incomingRelationItSystemUsage.Uuid, rmIncomingRelatedSystemUsage.ItSystemUsageUuid);
            Assert.Equal(incomingRelationItSystem.Name, rmIncomingRelatedSystemUsage.ItSystemUsageName);
        }

        private static ItContractItSystemUsage AssociateContract(ItContract contract, ItSystemUsage systemUsage)
        {
            return new ItContractItSystemUsage
            {
                ItContractId = contract.Id,
                ItContract = contract,
                ItSystemUsageId = systemUsage.Id,
                ItSystemUsage = systemUsage
            };
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_ActiveAccordingToValidityPeriod_False_When_ExpirationDate_Is_Earlier_Than_Today()
        {
            //Act
            var readModel = Test_ActiveAccordingToValidityPeriod_Based_On_ExpirationDate(DateTime.Now.AddDays(-A<int>()));

            //Assert
            Assert.False(readModel.ActiveAccordingToValidityPeriod);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_ActiveAccordingToValidityPeriod_True_When_ExpirationDate_Is_Today()
        {
            //Act
            var readModel = Test_ActiveAccordingToValidityPeriod_Based_On_ExpirationDate(DateTime.Now);

            //Assert
            Assert.True(readModel.ActiveAccordingToValidityPeriod);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_ActiveAccordingToValidityPeriod_True_When_ExpirationDate_Is_Later_Than_Today()
        {
            //Act
            var readModel = Test_ActiveAccordingToValidityPeriod_Based_On_ExpirationDate(DateTime.Now.AddDays(A<int>()));

            //Assert
            Assert.True(readModel.ActiveAccordingToValidityPeriod);
        }

        [Fact]
        public void Apply_Generates_Read_Model_When_No_Parent_System()
        {
            //Arrange
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                ObjectOwner = DefaultTestUser,
                LastChangedByUser = DefaultTestUser,
                LastChanged = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>()
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            Assert.Null(readModel.ParentItSystemName);
            Assert.Null(readModel.ParentItSystemId);
            Assert.Null(readModel.ParentItSystemUsageUuid);
        }

        [Fact]
        public void Apply_Generates_ArchivePeriodEndDate_By_Earliest_StartDate_Of_Still_Valid_ArchivePeriod()
        {
            //Arrange
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                ItSystem = system,
                ObjectOwner = DefaultTestUser,
                LastChangedByUser = DefaultTestUser,
                LastChanged = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>()
            };
            var earliestStartDate = DateTime.Now.AddYears(-1);
            var endDateOfEarlistStartDate = DateTime.Now.AddDays(A<int>());
            var archivePeriods = new List<ArchivePeriod>
            {
                new ArchivePeriod
                {
                    Id = A<int>(),
                    ItSystemUsage = systemUsage,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(1)
                },
                new ArchivePeriod
                {
                    Id = A<int>(),
                    ItSystemUsage = systemUsage,
                    StartDate = earliestStartDate,
                    EndDate = endDateOfEarlistStartDate
                },
                new ArchivePeriod
                {
                    Id = A<int>(),
                    ItSystemUsage = systemUsage,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = endDateOfEarlistStartDate.AddDays(A<int>())
                },
            };
            systemUsage.ArchivePeriods = archivePeriods;

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            Assert.Equal(endDateOfEarlistStartDate, readModel.ActiveArchivePeriodEndDate);
        }

        [Fact]
        public void Apply_Generates_RiskSupervisionDocumentation_As_Null_If_RiskAssessment_Is_Not_Yes()
        {
            //Arrange
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                ObjectOwner = DefaultTestUser,
                LastChangedByUser = DefaultTestUser,
                LastChanged = A<DateTime>(),
                riskAssessment = DataOptions.DONTKNOW,
                RiskSupervisionDocumentationUrlName = A<string>(),
                RiskSupervisionDocumentationUrl = A<string>(),
                PlannedRiskAssessmentDate = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>()
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            Assert.Null(readModel.RiskSupervisionDocumentationName);
            Assert.Null(readModel.RiskSupervisionDocumentationUrl);
        }

        [Fact]
        public void Apply_Generates_DataProcessingRegistrationCsv_Correctly_With_Multiple_DataProcessingRegistrations()
        {
            //Arrange
            var dpr1 = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                IsAgreementConcluded = YesNoIrrelevantOption.YES
            };
            var dpr2 = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                IsAgreementConcluded = YesNoIrrelevantOption.NO
            };
            var dpr3 = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                IsAgreementConcluded = YesNoIrrelevantOption.IRRELEVANT
            };
            var dpr4 = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                IsAgreementConcluded = YesNoIrrelevantOption.UNDECIDED
            };
            var dpr5 = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                IsAgreementConcluded = YesNoIrrelevantOption.UNDECIDED
            };
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                ObjectOwner = DefaultTestUser,
                LastChangedByUser = DefaultTestUser,
                LastChanged = A<DateTime>(),
                riskAssessment = DataOptions.DONTKNOW,
                RiskSupervisionDocumentationUrlName = A<string>(),
                RiskSupervisionDocumentationUrl = A<string>(),
                PlannedRiskAssessmentDate = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>()
                {
                    dpr1,
                    dpr2,
                    dpr3,
                    dpr4,
                    dpr5
                }
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            Assert.Equal($"{dpr1.Name}, {dpr2.Name}, {dpr3.Name}, {dpr4.Name}, {dpr5.Name}", readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal($"{dpr1.IsAgreementConcluded.GetValueOrDefault().GetReadableName()}, " +
                $"{dpr2.IsAgreementConcluded.GetValueOrDefault().GetReadableName()}, " +
                $"{dpr3.IsAgreementConcluded.GetValueOrDefault().GetReadableName()}", readModel.DataProcessingRegistrationsConcludedAsCsv);
        }

        [Fact]
        public void Apply_Generates_HostedAt_As_UNDECIDED_If_HostedAt_Is_Null()
        {
            //Arrange
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                ObjectOwner = DefaultTestUser,
                LastChangedByUser = DefaultTestUser,
                LastChanged = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>(),
                HostedAt = null
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            Assert.Equal(HostedAt.UNDECIDED, readModel.HostedAt);
        }

        [Fact]
        public void Apply_Generates_UserCount_As_UNDECIDED_If_UserCount_Is_Null()
        {
            //Arrange
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                ObjectOwner = DefaultTestUser,
                LastChangedByUser = DefaultTestUser,
                LastChanged = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>(),
                UserCount = null
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            Assert.Equal(UserCount.UNDECIDED, readModel.UserCount);
        }

        private ItSystemUsageOverviewReadModel Test_ActiveAccordingToValidityPeriod_Based_On_ExpirationDate(DateTime expirationDate)
        {
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                ExpirationDate = expirationDate,
                ObjectOwner = DefaultTestUser,
                LastChangedByUser = DefaultTestUser,
                LastChanged = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>()
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            _sut.Apply(systemUsage, readModel);

            return readModel;
        }

        private static void AssertOrgUnitUsage(ItSystemUsageOrgUnitUsage expected, ItSystemUsageOverviewReadModel actual)
        {
            AssertOrgUnitUsage(expected, (actual.ResponsibleOrganizationUnitId.GetValueOrDefault(), actual.ResponsibleOrganizationUnitName, actual.ResponsibleOrganizationUnitUuid.GetValueOrDefault()));
        }
        private static void AssertOrgUnitUsage(ItSystemUsageOrgUnitUsage expected, ItSystemUsageOverviewRelevantOrgUnitReadModel actual)
        {
            AssertOrgUnitUsage(expected, (actual.OrganizationUnitId, actual.OrganizationUnitName, actual.OrganizationUnitUuid));
        }

        private static void AssertOrgUnitUsage(ItSystemUsageOrgUnitUsage expected, (int id, string name, Guid uuid) actual)
        {
            Assert.Equal(expected.OrganizationUnit.Id, actual.id);
            Assert.Equal(expected.OrganizationUnit.Uuid, actual.uuid);
            Assert.Equal(expected.OrganizationUnit.Name, actual.name);
        }

        private ItSystemUsageOrgUnitUsage CreateOrganizationUnitUsage(ItSystemUsage systemUsage)
        {
            var organizationUnit = new OrganizationUnit
            {
                Id = A<int>(),
                Name = A<string>(),
                Uuid = A<Guid>()
            };
            var systemUsageOrgUnitUsage = new ItSystemUsageOrgUnitUsage
            {
                OrganizationUnit = organizationUnit,
                OrganizationUnitId = organizationUnit.Id,
                ItSystemUsage = systemUsage,
                ItSystemUsageId = systemUsage.Id
            };
            return systemUsageOrgUnitUsage;
        }
    }
}
