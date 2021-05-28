using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
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
using Infrastructure.Services.Types;
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
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewItProjectReadModel>> _itProjectReadModelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel>> _archivePeriodReadModelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel>> _dataProcessingReadModelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewInterfaceReadModel>> _interfacesReadModelRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewUsedBySystemUsageReadModel>> _itSystemUsageReadModelRepository;
        private readonly ItSystemUsageOverviewReadModelUpdate _sut;

        public ItSystemUsageOverviewReadModelUpdateTest()
        {
            _businessTypeService = new Mock<IOptionsService<ItSystem, BusinessType>>();
            _taskRefRepository = new Mock<IGenericRepository<ItSystemUsageOverviewTaskRefReadModel>>();
            _sensitiveDataLevelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewSensitiveDataLevelReadModel>>();
            _roleAssignmentRepository = new Mock<IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel>>();
            _itProjectReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewItProjectReadModel>>();
            _archivePeriodReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel>>();
            _dataProcessingReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel>>();
            _interfacesReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewInterfaceReadModel>>();
            _itSystemUsageReadModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewUsedBySystemUsageReadModel>>();
            _sut = new ItSystemUsageOverviewReadModelUpdate(
                _roleAssignmentRepository.Object,
                _taskRefRepository.Object,
                _sensitiveDataLevelRepository.Object,
                _itProjectReadModelRepository.Object,
                _archivePeriodReadModelRepository.Object,
                _dataProcessingReadModelRepository.Object,
                _interfacesReadModelRepository.Object,
                _itSystemUsageReadModelRepository.Object,
                _businessTypeService.Object);
        }

        public static User defaultTestUser = new User
        {
            Id = 1,
            Name = "test",
            LastName = "tester",
            Email = $"test@tester.dk"
        };

        [Fact]
        public void Apply_Generates_Correct_Read_Model()
        {
            //Arrange
            var outgoingRelationItSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
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
                RelationInterface = outgoingRelationInterface
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
                RoleId = A<int>()
            };
            var contract = new ItContract
            {
                Id = A<int>(),
                Name = A<string>(),
                Supplier = supplier
            };
            var project = new ItProject
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var dataProcessingRegistration = new DataProcessingRegistration()
            {
                Id = A<int>(),
                Name = A<string>(),
                IsAgreementConcluded = A<YesNoIrrelevantOption>()
            };
            var parentSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>()
            };
            var system = new ItSystem
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>(),
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
                OrganizationId = A<int>(),
                ItSystem = system,
                Active = A<bool>(),
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
                ItProjects = new List<ItProject> 
                { 
                    project 
                },
                ArchiveDuty = A<ArchiveDutyTypes>(),
                Registertype = A<bool>(),
                riskAssessment = DataOptions.YES,
                RiskSupervisionDocumentationUrlName = A<string>(),
                RiskSupervisionDocumentationUrl = A<string>(),
                LinkToDirectoryUrlName = A<string>(),
                LinkToDirectoryUrl = A<string>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>
                {
                    dataProcessingRegistration
                },
                GeneralPurpose = A<string>(),
                HostedAt = A<HostedAt>(),
                UsageRelations = new List<SystemRelation>
                {
                    outgoingRelation
                },
                UsedByRelations = new List<SystemRelation>
                {
                    incomingRelation
                }
            };

            // Add ResponsibleOrganizationUnit
            var organizationUnit = new OrganizationUnit
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsageOrgUnitUsage = new ItSystemUsageOrgUnitUsage
            {
                OrganizationUnit = organizationUnit,
                OrganizationUnitId = organizationUnit.Id,
                ItSystemUsage = systemUsage,
                ItSystemUsageId = systemUsage.Id
            };
            systemUsage.ResponsibleUsage = systemUsageOrgUnitUsage;

            _businessTypeService.Setup(x => x.GetOption(system.OrganizationId, system.BusinessType.Id)).Returns(Maybe<(BusinessType, bool)>.Some((system.BusinessType, true)));

            // Add MainContract
            var mainContract = new ItContractItSystemUsage
            {
                ItContractId = contract.Id,
                ItContract = contract,
                ItSystemUsageId = systemUsage.Id,
                ItSystemUsage = systemUsage
            };
            systemUsage.MainContract = mainContract;

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
                new ArchivePeriod
                {
                    Id = A<int>(),
                    ItSystemUsage = systemUsage,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(1)
                }
            };
            systemUsage.ArchivePeriods = archivePeriods;

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            //System usage
            Assert.Equal(systemUsage.Id, readModel.SourceEntityId);
            Assert.Equal(systemUsage.OrganizationId, readModel.OrganizationId);
            Assert.Equal(systemUsage.IsActive, readModel.IsActive);
            Assert.Equal(systemUsage.Version, readModel.Version);
            Assert.Equal(systemUsage.LocalCallName, readModel.LocalCallName);
            Assert.Equal(systemUsage.LocalSystemId, readModel.LocalSystemId);
            Assert.Equal(user.Id, readModel.ObjectOwnerId);
            Assert.Equal(user.GetFullName(), readModel.ObjectOwnerName);
            Assert.Equal(user.Id, readModel.LastChangedById);
            Assert.Equal(user.GetFullName(), readModel.LastChangedByName);
            Assert.Equal(systemUsage.LastChanged, readModel.LastChangedAt);
            Assert.Equal(systemUsage.Concluded, readModel.Concluded);
            Assert.Equal(systemUsage.ArchiveDuty, readModel.ArchiveDuty);
            Assert.Equal(systemUsage.Registertype, readModel.IsHoldingDocument);
            Assert.Equal(systemUsage.RiskSupervisionDocumentationUrlName, readModel.RiskSupervisionDocumentationName);
            Assert.Equal(systemUsage.RiskSupervisionDocumentationUrl, readModel.RiskSupervisionDocumentationUrl);
            Assert.Equal(systemUsage.LinkToDirectoryUrlName, readModel.LinkToDirectoryName);
            Assert.Equal(systemUsage.LinkToDirectoryUrl, readModel.LinkToDirectoryUrl);
            Assert.Equal(systemUsage.GeneralPurpose, readModel.GeneralPurpose);
            Assert.Equal(systemUsage.HostedAt, readModel.HostedAt);

            // Sensitive data levels
            var rmSensitiveDataLevel = Assert.Single(readModel.SensitiveDataLevels);
            Assert.Equal(sensitiveDataLevel.SensitivityDataLevel, rmSensitiveDataLevel.SensitivityDataLevel);
            Assert.Equal(sensitiveDataLevel.SensitivityDataLevel.GetReadableName(), readModel.SensitiveDataLevelsAsCsv);

            //System
            Assert.Equal(system.Name, readModel.SystemName);
            Assert.Equal(system.Disabled, readModel.ItSystemDisabled);
            Assert.Equal(system.Uuid.ToString("D"), readModel.ItSystemUuid);
            Assert.Equal(system.BelongsTo.Id, readModel.ItSystemRightsHolderId);
            Assert.Equal(system.BelongsTo.Name, readModel.ItSystemRightsHolderName);
            Assert.Equal(system.BusinessType.Id, readModel.ItSystemBusinessTypeId);
            Assert.Equal(system.BusinessType.Name, readModel.ItSystemBusinessTypeName);

            //Parent System
            Assert.Equal(parentSystem.Name, readModel.ParentItSystemName);
            Assert.Equal(parentSystem.Id, readModel.ParentItSystemId);
            Assert.Equal(parentSystem.Disabled, readModel.ParentItSystemDisabled);

            //Assigned Roles
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.GetFullName(), roleAssignment.UserFullName);
            Assert.Equal(right.RoleId, roleAssignment.RoleId);
            Assert.Equal(user.Email, roleAssignment.Email);

            //Responsible Organization Unit
            Assert.Equal(organizationUnit.Id, readModel.ResponsibleOrganizationUnitId);
            Assert.Equal(organizationUnit.Name, readModel.ResponsibleOrganizationUnitName);

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
            Assert.Equal(contract.Id, readModel.MainContractId);
            Assert.Equal(contract.Supplier.Id, readModel.MainContractSupplierId);
            Assert.Equal(contract.Supplier.Name, readModel.MainContractSupplierName);
            Assert.Equal(contract.IsActive, readModel.MainContractIsActive.GetValueOrDefault(false));
            Assert.True(readModel.HasMainContract);

            //Projects
            Assert.Equal(project.Name, readModel.ItProjectNamesAsCsv);
            var rmProject = Assert.Single(readModel.ItProjects);
            Assert.Equal(project.Id, rmProject.ItProjectId);
            Assert.Equal(project.Name, rmProject.ItProjectName);

            //ArchivePeriods
            var rmArchivePeriod = Assert.Single(readModel.ArchivePeriods);
            Assert.Equal(archivePeriods.First().StartDate, rmArchivePeriod.StartDate);
            Assert.Equal(archivePeriods.First().EndDate, rmArchivePeriod.EndDate);
            Assert.Equal(archivePeriods.First().EndDate, readModel.ActiveArchivePeriodEndDate);

            //DataProcessingRegistrations
            Assert.Equal(dataProcessingRegistration.Name, readModel.DataProcessingRegistrationNamesAsCsv);
            Assert.Equal(dataProcessingRegistration.IsAgreementConcluded.GetValueOrDefault(YesNoIrrelevantOption.UNDECIDED).GetReadableName(), readModel.DataProcessingRegistrationsConcludedAsCsv);
            var rmDataProcessingRegistration = Assert.Single(readModel.DataProcessingRegistrations);
            Assert.Equal(dataProcessingRegistration.Name, rmDataProcessingRegistration.DataProcessingRegistrationName);
            Assert.Equal(dataProcessingRegistration.IsAgreementConcluded, rmDataProcessingRegistration.IsAgreementConcluded);

            //Outgoing Relations
            Assert.Equal(outgoingRelationInterface.Name, readModel.DependsOnInterfacesNamesAsCsv);
            var rmDependsOnInterface = Assert.Single(readModel.DependsOnInterfaces);
            Assert.Equal(outgoingRelationInterface.Id, rmDependsOnInterface.InterfaceId);
            Assert.Equal(outgoingRelationInterface.Name, rmDependsOnInterface.InterfaceName);

            //Incoming Relations
            Assert.Equal(incomingRelationItSystem.Name, readModel.IncomingRelatedItSystemUsagesNamesAsCsv);
            var rmIncomingRelatedSystemUsage = Assert.Single(readModel.IncomingRelatedItSystemUsages);
            Assert.Equal(incomingRelationItSystemUsage.Id, rmIncomingRelatedSystemUsage.ItSystemUsageId);
            Assert.Equal(incomingRelationItSystem.Name, rmIncomingRelatedSystemUsage.ItSystemUsageName);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_IsActive_False_When_ExpirationDate_Is_Earlier_Than_Today()
        {
            //Act
            var readModel = Test_IsActive_Based_On_ExpirationDate(DateTime.Now.AddDays(-A<int>()));

            //Assert
            Assert.False(readModel.IsActive);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_IsActive_False_When_ExpirationDate_Is_Today()
        {
            //Act
            var readModel = Test_IsActive_Based_On_ExpirationDate(DateTime.Now);

            //Assert
            Assert.False(readModel.IsActive);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_IsActive_False_When_ExpirationDate_Is_Later_Than_Today()
        {
            //Act
            var readModel = Test_IsActive_Based_On_ExpirationDate(DateTime.Now.AddDays(A<int>()));

            //Assert
            Assert.False(readModel.IsActive);
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
                ObjectOwner = defaultTestUser,
                LastChangedByUser = defaultTestUser,
                LastChanged = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>()
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            Assert.Null(readModel.ParentItSystemName);
            Assert.Null(readModel.ParentItSystemId);
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
                ObjectOwner = defaultTestUser,
                LastChangedByUser = defaultTestUser,
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
                ObjectOwner = defaultTestUser,
                LastChangedByUser = defaultTestUser,
                LastChanged = A<DateTime>(),
                riskAssessment = DataOptions.DONTKNOW,
                RiskSupervisionDocumentationUrlName = A<string>(),
                RiskSupervisionDocumentationUrl = A<string>(),
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
                ObjectOwner = defaultTestUser,
                LastChangedByUser = defaultTestUser,
                LastChanged = A<DateTime>(),
                riskAssessment = DataOptions.DONTKNOW,
                RiskSupervisionDocumentationUrlName = A<string>(),
                RiskSupervisionDocumentationUrl = A<string>(),
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
                ObjectOwner = defaultTestUser,
                LastChangedByUser = defaultTestUser,
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

        private ItSystemUsageOverviewReadModel Test_IsActive_Based_On_ExpirationDate(DateTime expirationDate)
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
                Active = false,
                ExpirationDate = DateTime.Now.AddDays(-1),
                ObjectOwner = defaultTestUser,
                LastChangedByUser = defaultTestUser,
                LastChanged = A<DateTime>(),
                AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>()
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            _sut.Apply(systemUsage, readModel);

            return readModel;
        }
    }
}
