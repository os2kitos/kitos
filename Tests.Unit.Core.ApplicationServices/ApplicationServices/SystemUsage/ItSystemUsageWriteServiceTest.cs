using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Options;
using Core.DomainServices.Role;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Moq;
using Moq.Language.Flow;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageWriteServiceTest : WithAutoFixture
    {
        private readonly Mock<IItSystemUsageService> _itSystemUsageServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOptionsService<ItSystemUsage, ItSystemCategories>> _systemCategoriesOptionsServiceMock;
        private readonly Mock<IOptionsService<ItSystemUsage, ArchiveType>> _archiveTypeOptionsServiceMock;
        private readonly Mock<IOptionsService<ItSystemUsage, ArchiveLocation>> _archiveLocationOptionsServiceMock;
        private readonly Mock<IOptionsService<ItSystemUsage, ArchiveTestLocation>> _archiveTestLocationOptionsServiceMock;
        private readonly Mock<IItContractService> _contractServiceMock;
        private readonly Mock<IItProjectService> _projectServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>> _roleAssignmentService;
        private readonly ItSystemUsageWriteService _sut;
        private readonly Mock<IKLEApplicationService> _kleServiceMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;
        private readonly Mock<IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>> _sensitiveDataOptionsService;
        private readonly Mock<IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage>> _registerTypeOptionsService;
        private readonly Mock<IGenericRepository<ItSystemUsageSensitiveDataLevel>> _sensitiveDataLevelRepository;

        public ItSystemUsageWriteServiceTest()
        {
            _itSystemUsageServiceMock = new Mock<IItSystemUsageService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _systemCategoriesOptionsServiceMock = new Mock<IOptionsService<ItSystemUsage, ItSystemCategories>>();
            _archiveTypeOptionsServiceMock = new Mock<IOptionsService<ItSystemUsage, ArchiveType>>();
            _archiveLocationOptionsServiceMock = new Mock<IOptionsService<ItSystemUsage, ArchiveLocation>>();
            _archiveTestLocationOptionsServiceMock = new Mock<IOptionsService<ItSystemUsage, ArchiveTestLocation>>();
            _contractServiceMock = new Mock<IItContractService>();
            _projectServiceMock = new Mock<IItProjectService>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _kleServiceMock = new Mock<IKLEApplicationService>();
            _referenceServiceMock = new Mock<IReferenceService>();
            _roleAssignmentService = new Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>>();
            _sensitiveDataOptionsService = new Mock<IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>>();
            _registerTypeOptionsService = new Mock<IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage>>();
            _sensitiveDataLevelRepository = new Mock<IGenericRepository<ItSystemUsageSensitiveDataLevel>>();
            _sut = new ItSystemUsageWriteService(_itSystemUsageServiceMock.Object, _transactionManagerMock.Object,
                _itSystemServiceMock.Object, _organizationServiceMock.Object, _authorizationContextMock.Object,
                _systemCategoriesOptionsServiceMock.Object, _contractServiceMock.Object, _projectServiceMock.Object,
                _kleServiceMock.Object, _referenceServiceMock.Object, _roleAssignmentService.Object,
                _sensitiveDataOptionsService.Object,
                _registerTypeOptionsService.Object,
                _sensitiveDataLevelRepository.Object,
                Mock.Of<IDatabaseControl>(), _domainEventsMock.Object, Mock.Of<ILogger>(),
                _archiveTypeOptionsServiceMock.Object, _archiveLocationOptionsServiceMock.Object,
                _archiveTestLocationOptionsServiceMock.Object,
                Mock.Of<IItsystemUsageRelationsService>(),
                Mock.Of<IEntityIdentityResolver>());
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            var outerFixture = new Fixture();

            //Ensure operation errors are always auto-created WITH both failure and message
            fixture.Register(() => new OperationError(outerFixture.Create<string>(), outerFixture.Create<OperationFailure>()));
        }

        [Fact]
        public void Can_Create_Without_Additional_Data()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var organization = CreateOrganization();
            var itSystem = new ItSystem { Id = A<int>() };
            var itSystemUsage = new ItSystemUsage();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            Assert.True(createResult.Ok);
            Assert.Same(itSystemUsage, createResult.Value);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Create_Fails_With_Usage_CreationFailure()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var organization = CreateOrganization();
            var itSystem = new ItSystem { Id = A<int>() };
            var error = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetSystemReturns(systemUuid, itSystem);
            ExpectCreateNewReturns(itSystem, organization, error);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            AssertFailureWithExpectedOperationError(createResult, error, transactionMock);
        }

        [Fact]
        public void Create_Fails_GetOrganization_Failure()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var error = A<OperationError>();

            ExpectGetSystemReturns(systemUuid, error);
            ExpectGetOrganizationReturns(organizationUuid, error);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            AssertFailureWithExpectedOperationError(createResult, error, transactionMock);
        }

        [Fact]
        public void Create_Fails_GetSystem_Failure()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var error = A<OperationError>();

            ExpectGetSystemReturns(systemUuid, error);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            AssertFailureWithExpectedOperationError(createResult, error, transactionMock);
        }

        [Theory]
        [InlineData(0, 9, UserCount.BELOWTEN)]
        [InlineData(10, 50, UserCount.TENTOFIFTY)]
        [InlineData(50, 100, UserCount.FIFTYTOHUNDRED)]
        [InlineData(100, null, UserCount.HUNDREDPLUS)]
        public void Can_Create_With_General_Data_With_All_Data_Defined(int minimumNumberOfUsers, int? maxNumberOfUsers, UserCount expectedNumberOfUsers)
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var newContractId = A<Guid>();
            var newContract = CreateItContract(organization, newContractId);
            itSystemUsage.Contracts.Add(CreateContractAssociation(organization));
            itSystemUsage.Contracts.Add(CreateContractAssociation(organization, newContract));
            var projectUuids = Many<Guid>().ToList();
            var dataClassificationId = A<Guid>();
            var itSystemCategories = new ItSystemCategories { Id = A<int>(), Uuid = dataClassificationId };
            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    LocalCallName = A<string>().AsChangedValue(),
                    LocalSystemId = A<string>().AsChangedValue(),
                    SystemVersion = A<string>().AsChangedValue(),
                    Notes = A<string>().AsChangedValue(),
                    EnforceActive = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                    DataClassificationUuid = Maybe<Guid>.Some(dataClassificationId).AsChangedValue(),
                    ValidFrom = Maybe<DateTime>.Some(DateTime.Now).AsChangedValue(),
                    ValidTo = Maybe<DateTime>.Some(DateTime.Now.AddDays(Math.Abs(A<short>()))).AsChangedValue(),
                    MainContractUuid = Maybe<Guid>.Some(newContractId).AsChangedValue(),
                    AssociatedProjectUuids = Maybe<IEnumerable<Guid>>.Some(projectUuids).AsChangedValue(),
                    NumberOfExpectedUsersInterval = Maybe<(int lower, int? upperBound)>.Some((minimumNumberOfUsers, maxNumberOfUsers)).AsChangedValue(),
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectGetItSystemCategoryReturns(itSystemUsage.OrganizationId, dataClassificationId, (itSystemCategories, true));
            ExpectGetContractReturns(newContractId, newContract);
            foreach (var projectUuid in projectUuids)
                ExpectGetProjectReturns(projectUuid, CreateItProject(organization, projectUuid));

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            Assert.Same(itSystemUsage, createResult.Value);
            AssertTransactionCommitted(transactionMock);
            Assert.Equal(expectedNumberOfUsers, itSystemUsage.UserCount);
            var generalProperties = input.GeneralProperties.Value;
            Assert.Equal(generalProperties.LocalCallName.NewValue, itSystemUsage.LocalCallName);
            Assert.Equal(generalProperties.LocalSystemId.NewValue, itSystemUsage.LocalSystemId);
            Assert.Equal(generalProperties.SystemVersion.NewValue, itSystemUsage.Version);
            Assert.Equal(generalProperties.Notes.NewValue, itSystemUsage.Note);
            Assert.Equal(generalProperties.EnforceActive.NewValue.Value, itSystemUsage.Active);
            Assert.Equal(generalProperties.DataClassificationUuid.NewValue.Value, itSystemUsage.ItSystemCategories.Uuid);
            Assert.Equal(generalProperties.ValidFrom.NewValue.Value.Date, itSystemUsage.Concluded);
            Assert.Equal(generalProperties.ValidTo.NewValue.Value.Date, itSystemUsage.ExpirationDate);
            Assert.Equal(generalProperties.MainContractUuid.NewValue.Value, itSystemUsage.MainContract.ItContract.Uuid);
            Assert.Equal(projectUuids.OrderBy(x => x).ToList(), itSystemUsage.ItProjects.OrderBy(x => x.Uuid).Select(x => x.Uuid).ToList());
        }

        [Fact]
        public void Cannot_Create_With_UnAvailable_SystemCategory()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var dataClassificationId = A<Guid>();
            var itSystemCategories = new ItSystemCategories { Id = A<int>(), Uuid = dataClassificationId };
            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    DataClassificationUuid = Maybe<Guid>.Some(dataClassificationId).AsChangedValue(),
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectGetItSystemCategoryReturns(itSystemUsage.OrganizationId, dataClassificationId, (itSystemCategories, false));

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Unknown_SystemCategory()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var dataClassificationId = A<Guid>();
            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    DataClassificationUuid = Maybe<Guid>.Some(dataClassificationId).AsChangedValue(),
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectGetItSystemCategoryReturns(itSystemUsage.OrganizationId, dataClassificationId, Maybe<(ItSystemCategories, bool)>.None);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_Contract_ErrorResponse()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var newContractId = A<Guid>();
            var operationError = A<OperationError>();
            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    MainContractUuid = Maybe<Guid>.Some(newContractId).AsChangedValue()
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectGetContractReturns(newContractId, operationError);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailureWithExpectedOperationError(createResult, operationError, transactionMock);
        }

        [Fact]
        public void Cannot_Create_If_SelectedMainContract_Is_Not_In_Same_Organization_As_ItSystemUsage()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var wrongOrganization = CreateOrganization();
            var newContractId = A<Guid>();
            var contract = CreateItContract(wrongOrganization, newContractId);

            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    MainContractUuid = Maybe<Guid>.Some(newContractId).AsChangedValue()
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectGetContractReturns(newContractId, contract);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_If_SelectedMainContract_Is_Not_Associated_To_ItSystemUsage()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            itSystemUsage.Contracts.Add(CreateContractAssociation(organization));
            var newContractId = A<Guid>();
            var contract = CreateItContract(organization, newContractId);

            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    MainContractUuid = Maybe<Guid>.Some(newContractId).AsChangedValue()
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectGetContractReturns(newContractId, contract);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_If_Projects_Are_Not_Unique()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var projectUuids = Many<Guid>().ToList();
            projectUuids.Add(projectUuids.Last()); //add a duplicatge
            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    AssociatedProjectUuids = Maybe<IEnumerable<Guid>>.Some(projectUuids).AsChangedValue(),
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            foreach (var projectUuid in projectUuids)
                ExpectGetProjectReturns(projectUuid, CreateItProject(organization, projectUuid));

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_If_Project_Is_In_Wrong_Organization()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var wrongOrganization = CreateOrganization();
            var projectUuids = Many<Guid>().ToList();
            var badProjectUuid = A<Guid>();
            projectUuids.Add(badProjectUuid); //Add a project which is in the wrong org (not the same as itsystem usage)
            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    AssociatedProjectUuids = Maybe<IEnumerable<Guid>>.Some(projectUuids).AsChangedValue(),
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            foreach (var projectUuid in projectUuids)
                ExpectGetProjectReturns(projectUuid, CreateItProject(projectUuid == badProjectUuid ? wrongOrganization : organization, projectUuid));

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_If_ValidFrom_Is_After_ValidTo()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var validFrom = Maybe<DateTime>.Some(DateTime.Now).AsChangedValue();
            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    ValidFrom = validFrom,
                    ValidTo = validFrom.Value.Select(x => x.Date.AddDays(-1)).AsChangedValue()
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Theory]
        [InlineData(-1, null)]
        [InlineData(0, null)]
        [InlineData(101, 102)]
        public void Cannot_Create_If_User_Count_Us_Not_Supported(int lower, int? upper)
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    NumberOfExpectedUsersInterval = (lower, upper).FromNullable().AsChangedValue()
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_WithoutDefinedChanges_Leaves_Properties_Untouched(bool entireSectionNone)
        {
            //Arrange - simulate that the created system has some initial values
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var localCallName = A<string>();
            var localSystemId = A<string>();
            var version = A<string>();
            var note = A<string>();
            var active = A<bool>();
            var concluded = A<DateTime>();
            var expirationDate = A<DateTime>();
            var userCount = A<UserCount>();
            var itSystemCategories = new ItSystemCategories();
            var itContractItSystemUsage = new ItContractItSystemUsage();
            var itProjects = Many<Guid>().Select(x => CreateItProject(organization, x)).ToList();

            itSystemUsage.LocalCallName = localCallName;
            itSystemUsage.LocalSystemId = localSystemId;
            itSystemUsage.Version = version;
            itSystemUsage.Note = note;
            itSystemUsage.Active = active;
            itSystemUsage.Concluded = concluded;
            itSystemUsage.ExpirationDate = expirationDate;
            itSystemUsage.UserCount = userCount;
            itSystemUsage.ItSystemCategories = itSystemCategories;
            itSystemUsage.MainContract = itContractItSystemUsage;
            itSystemUsage.ItProjects = itProjects;

            var input = new SystemUsageUpdateParameters
            {
                GeneralProperties = entireSectionNone ?
                    Maybe<UpdatedSystemUsageGeneralProperties>.None : //No changes on the root level of the section
                    new UpdatedSystemUsageGeneralProperties() //No changes defined within should have the same effect as the None case
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert that values have not been modified
            Assert.True(createResult.Ok);
            Assert.Same(itSystemUsage, createResult.Value);
            AssertTransactionCommitted(transactionMock);
            Assert.Equal(localCallName, itSystemUsage.LocalCallName);
            Assert.Equal(localSystemId, itSystemUsage.LocalSystemId);
            Assert.Equal(version, itSystemUsage.Version);
            Assert.Equal(note, itSystemUsage.Note);
            Assert.Equal(active, itSystemUsage.Active);
            Assert.Equal(expirationDate, itSystemUsage.ExpirationDate);
            Assert.Equal(concluded, itSystemUsage.Concluded);
            Assert.Equal(userCount, itSystemUsage.UserCount);
            Assert.Equal(itSystemCategories, itSystemUsage.ItSystemCategories);
            Assert.Equal(itContractItSystemUsage, itSystemUsage.MainContract);
            Assert.Equal(itProjects, itSystemUsage.ItProjects);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OrganizationalUsage(bool withResponsible)
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var organizationUnits = Many<Guid>().Select(uuid => CreateOrganizationUnit(uuid, organization)).ToList();
            var responsible = withResponsible ? organizationUnits.OrderBy(_ => A<int>()).First() : null;

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            foreach (var organizationUnit in organizationUnits)
                ExpectGetOrganizationUnitReturns(organizationUnit.Uuid, organizationUnit);

            var input = new SystemUsageUpdateParameters
            {
                OrganizationalUsage = new UpdatedSystemUsageOrganizationalUseParameters
                {
                    UsingOrganizationUnitUuids = organizationUnits.Select(x => x.Uuid).ToList().FromNullable<IEnumerable<Guid>>().AsChangedValue(),
                    ResponsibleOrganizationUnitUuid = (responsible?.Uuid.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                }
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            Assert.Same(itSystemUsage, createResult.Value);
            AssertTransactionCommitted(transactionMock);
            Assert.Equal(organizationUnits, itSystemUsage.UsedBy.Select(x => x.OrganizationUnit));
            if (withResponsible)
            {
                Assert.NotNull(itSystemUsage.ResponsibleUsage);
                Assert.Equal(responsible, itSystemUsage.ResponsibleUsage.OrganizationUnit);
            }
            else
            {
                Assert.Null(itSystemUsage.ResponsibleUsage);
            }
        }

        [Fact]
        public void CannotCreate_With_OrganizationalUsage_When_Responsible_Usage_Is_Not_Part_Of_Using_Orgs()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var organizationUnits = Many<Guid>().Select(uuid => CreateOrganizationUnit(uuid, organization)).ToList();
            var responsible = organizationUnits.Take(1).Single();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            foreach (var organizationUnit in organizationUnits)
                ExpectGetOrganizationUnitReturns(organizationUnit.Uuid, organizationUnit);

            var input = new SystemUsageUpdateParameters
            {
                OrganizationalUsage = new UpdatedSystemUsageOrganizationalUseParameters
                {
                    //Skip the "responsible" from the unit-user list
                    UsingOrganizationUnitUuids = organizationUnits.Skip(1).Select(x => x.Uuid).ToList().FromNullable<IEnumerable<Guid>>().AsChangedValue(),
                    ResponsibleOrganizationUnitUuid = responsible.Uuid.FromNullable().AsChangedValue()
                }
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void CannotCreate_With_OrganizationalUsage_When_UsingOrgs_Contains_Duplicates()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var organizationUnits = Many<Guid>().Select(uuid => CreateOrganizationUnit(uuid, organization)).ToList();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            foreach (var organizationUnit in organizationUnits)
                ExpectGetOrganizationUnitReturns(organizationUnit.Uuid, organizationUnit);

            var input = new SystemUsageUpdateParameters
            {
                OrganizationalUsage = new UpdatedSystemUsageOrganizationalUseParameters
                {
                    UsingOrganizationUnitUuids = organizationUnits.Concat(organizationUnits.Take(1)).Select(x => x.Uuid).ToList().FromNullable<IEnumerable<Guid>>().AsChangedValue(),
                }
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void CannotCreate_With_OrganizationalUsage_When_Org_Units_Are_In_Different_Organization_Than_Usage()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var anotherOrg = CreateOrganization();
            var organizationUnits = Many<Guid>().Select(uuid => CreateOrganizationUnit(uuid, anotherOrg)).ToList();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            foreach (var organizationUnit in organizationUnits)
                ExpectGetOrganizationUnitReturns(organizationUnit.Uuid, organizationUnit);

            var input = new SystemUsageUpdateParameters
            {
                OrganizationalUsage = new UpdatedSystemUsageOrganizationalUseParameters
                {
                    UsingOrganizationUnitUuids = organizationUnits.Select(x => x.Uuid).ToList().FromNullable<IEnumerable<Guid>>().AsChangedValue(),
                }
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, OperationFailure.BadInput, transactionMock);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Can_Create_With_KLEDeviations(bool withAdditions, bool withRemovals)
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            itSystem.TaskRefs = CreateTaskRefs(3);
            var additionalTaskRefs = CreateTaskRefs(withAdditions ? 2 : 0);
            var tasksToRemove = itSystem.TaskRefs.Take(withRemovals ? 2 : 0).ToList();


            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var input = SetupKLEInputExpectations(additionalTaskRefs, tasksToRemove);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
            Assert.Equal(additionalTaskRefs, itSystemUsage.TaskRefs);
            Assert.Equal(tasksToRemove, itSystemUsage.TaskRefsOptOut);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Cannot_Create_With_KLEDeviations_With_Duplicates(bool inAdditions, bool inRemovals)
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            itSystem.TaskRefs = CreateTaskRefs(3);
            var additionalTaskRefs = CreateTaskRefs(1).Transform(refs => inAdditions ? refs.Append(refs.First()) : refs).ToList();
            var tasksToRemove = itSystem.TaskRefs.Take(1).ToList().Transform(refs => inRemovals ? refs.Append(refs.First()) : refs).ToList();


            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var input = SetupKLEInputExpectations(additionalTaskRefs, tasksToRemove);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailureWithErrorMessageContent(createResult, OperationFailure.BadInput, "Duplicates in KLE", transactionMock);

        }

        [Fact]
        public void Cannot_Create_With_KLEDeviations_If_Additions_And_Removals_Intersect()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            itSystem.TaskRefs = CreateTaskRefs(3);
            var additionalTaskRefs = CreateTaskRefs(1).Append(itSystem.TaskRefs.First()).ToList();
            var tasksToRemove = itSystem.TaskRefs.Take(2).ToList(); //First one is in both collections


            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var input = SetupKLEInputExpectations(additionalTaskRefs, tasksToRemove);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailureWithErrorMessageContent(createResult, OperationFailure.BadInput, "KLE cannot be both added and removed", transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_KLEDeviations_If_Additions_Exist_In_SystemContext()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            itSystem.TaskRefs = CreateTaskRefs(3);
            var additionalTaskRefs = CreateTaskRefs(1).Append(itSystem.TaskRefs.First()).ToList(); //Add one from the system context

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var input = SetupKLEInputExpectations(additionalTaskRefs, Enumerable.Empty<TaskRef>().ToList());

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailureWithErrorMessageContent(createResult, OperationFailure.BadInput, "Cannot ADD KLE which is already present in the system context", transactionMock);

        }

        [Fact]
        public void Cannot_Create_With_KLEDeviations_If_RemovedKLE_Do_Not_Exist_On_System_Context()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            itSystem.TaskRefs = CreateTaskRefs(3);
            var tasksToRemove = itSystem.TaskRefs.Take(1).Concat(CreateTaskRefs(1)).ToList(); //One exists on system context and the other one does not


            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var input = SetupKLEInputExpectations(Enumerable.Empty<TaskRef>().ToList(), tasksToRemove);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailureWithErrorMessageContent(createResult, OperationFailure.BadInput, "Cannot Remove KLE which is not present in the system context", transactionMock);
        }

        [Fact]
        public void Can_Create_With_ExternalReferences()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var expectedMaster = externalReferences.OrderBy(x => A<int>()).First();
            expectedMaster.MasterReference = true;

            var input = new SystemUsageUpdateParameters
            {
                ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>()
            };
            foreach (var externalReference in externalReferences)
                ExpectAddExternalReferenceReturns(itSystemUsage, externalReference, CreateExternalReference(externalReference));

            //Act
            var result = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transactionMock);
            Assert.Equal(expectedMaster.Title, result.Value.Reference.Title);
        }

        [Fact]
        public void Cannot_Create_With_ExternalReferences_But_No_Master()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();

            var input = new SystemUsageUpdateParameters
            {
                ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>()
            };

            //Act
            var result = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(result, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_ExternalReferences_And_Multiple_Masters()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();

            //Set two masters
            foreach (var master in externalReferences.Take(2))
                master.MasterReference = true;

            var input = new SystemUsageUpdateParameters
            {
                ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>()
            };

            //Act
            var result = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(result, OperationFailure.BadInput, transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_ExternalReferences_If_Add_Reference_Fails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var expectedMaster = externalReferences.OrderBy(x => A<int>()).First();
            expectedMaster.MasterReference = true;
            var operationError = A<OperationError>();

            var input = new SystemUsageUpdateParameters
            {
                ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>()
            };
            foreach (var externalReference in externalReferences.Where(x => x.MasterReference == false))
                ExpectAddExternalReferenceReturns(itSystemUsage, externalReference, CreateExternalReference(externalReference));
            ExpectAddExternalReferenceReturns(itSystemUsage, expectedMaster, operationError);

            //Act
            var result = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailureWithExpectedOperationError(result, operationError, transactionMock);
        }

        [Fact]
        public void Can_Create_With_Archiving()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, true));
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectRemoveAllArchivePeriodsReturns(itSystemUsage.Id, new List<ArchivePeriod>());

            Configure(f => f.Inject(new ArchivePeriod()
            {
                Approved = A<bool>(),
                UniqueArchiveId = A<string>(),
                StartDate = A<DateTime>(),
                EndDate = A<DateTime>()
            }));
            ExpectAddArchivePeriodReturns(itSystemUsage.Id, Result<ArchivePeriod, OperationError>.Success(A<ArchivePeriod>()));

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertArchivingParameters(archivingParameters, organization.Name, organization.Id, createResult.Value);
        }

        [Fact]
        public void Can_Create_With_Archiving_With_Maybe_None_Values()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);


            var archivingParameters = CreateEmptySystemUsageArchivingParameters();
            ExpectRemoveAllArchivePeriodsReturns(itSystemUsage.Id, new List<ArchivePeriod>());

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertEmptyArchivingParameters(createResult.Value);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_ArchiveTypeUuid_Not_Exists()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, Maybe<(ArchiveType, bool)>.None);

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_ArchiveTypeUuid_Not_Available_In_Org()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, false));

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Can_Create_With_Archiving_If_ArchiveTypeUuid_Not_Available_In_Org_But_The_Value_Is_Not_Changed()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var archiveTypeUuid = A<Guid>();
            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            itSystemUsage.ArchiveTypeId = archiveType.Id;
            itSystemUsage.ArchiveType = archiveType;

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();
            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, false));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, true));
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectRemoveAllArchivePeriodsReturns(itSystemUsage.Id, new List<ArchivePeriod>());

            Configure(f => f.Inject(new ArchivePeriod()
            {
                Approved = A<bool>(),
                UniqueArchiveId = A<string>(),
                StartDate = A<DateTime>(),
                EndDate = A<DateTime>()
            }));
            ExpectAddArchivePeriodReturns(itSystemUsage.Id, Result<ArchivePeriod, OperationError>.Success(A<ArchivePeriod>()));


            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertArchivingParameters(archivingParameters, organization.Name, organization.Id, createResult.Value);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_ArchiveLocationUuid_Not_Exists()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, Maybe<(ArchiveLocation, bool)>.None);

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_ArchiveLocationUuid_Not_Available_In_Org()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, false));

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Can_Create_With_Archiving_If_ArchiveLocationUuid_Not_Available_In_Org_But_The_Value_Is_Not_Changed()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var archiveLocationUuid = A<Guid>();
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            itSystemUsage.ArchiveLocationId = archiveLocation.Id;
            itSystemUsage.ArchiveLocation = archiveLocation;

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();
            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, false));
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, true));
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectRemoveAllArchivePeriodsReturns(itSystemUsage.Id, new List<ArchivePeriod>());

            Configure(f => f.Inject(new ArchivePeriod()
            {
                Approved = A<bool>(),
                UniqueArchiveId = A<string>(),
                StartDate = A<DateTime>(),
                EndDate = A<DateTime>()
            }));
            ExpectAddArchivePeriodReturns(itSystemUsage.Id, Result<ArchivePeriod, OperationError>.Success(A<ArchivePeriod>()));


            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertArchivingParameters(archivingParameters, organization.Name, organization.Id, createResult.Value);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_ArchiveTestLocationUuid_Not_Exists()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, Maybe<(ArchiveTestLocation, bool)>.None);

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_ArchiveTestLocationUuid_Not_Available_In_Org()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, false));

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Can_Create_With_Archiving_If_ArchiveTestLocationUuid_Not_Available_In_Org_But_The_Value_Is_Not_Changed()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var archiveTestLocationUuid = A<Guid>();
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            itSystemUsage.ArchiveTestLocationId = archiveTestLocation.Id;
            itSystemUsage.ArchiveTestLocation = archiveTestLocation;

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, false));
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectRemoveAllArchivePeriodsReturns(itSystemUsage.Id, new List<ArchivePeriod>());

            Configure(f => f.Inject(new ArchivePeriod()
            {
                Approved = A<bool>(),
                UniqueArchiveId = A<string>(),
                StartDate = A<DateTime>(),
                EndDate = A<DateTime>()
            }));
            ExpectAddArchivePeriodReturns(itSystemUsage.Id, Result<ArchivePeriod, OperationError>.Success(A<ArchivePeriod>()));


            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertArchivingParameters(archivingParameters, organization.Name, organization.Id, createResult.Value);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_GetSupplierOrganizationByUuid_Fails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, true));
            var failure = A<OperationFailure>();
            ExpectGetOrganizationReturns(organizationUuid, new OperationError(failure));

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(failure, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_RemoveAllArchivePeriods_Fails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, true));
            ExpectGetOrganizationReturns(organizationUuid, organization);

            var failure = A<OperationFailure>();
            _itSystemUsageServiceMock.Setup(x => x.RemoveAllArchivePeriods(itSystemUsage.Id))
                .Returns(new OperationError(failure));

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(failure, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Archiving_If_AddArchivePeriod_Fails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var archiveTypeUuid = A<Guid>();
            var archiveLocationUuid = A<Guid>();
            var archiveTestLocationUuid = A<Guid>();

            var archivingParameters = CreateSystemUsageArchivingParameters(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, organizationUuid);

            var archiveType = new ArchiveType() { Id = A<int>(), Uuid = archiveTypeUuid };
            ExpectGetArchiveTypeReturns(organization.Id, archiveTypeUuid, (archiveType, true));
            var archiveLocation = new ArchiveLocation() { Id = A<int>(), Uuid = archiveLocationUuid };
            ExpectGetArchiveLocationReturns(organization.Id, archiveLocationUuid, (archiveLocation, true));
            var archiveTestLocation = new ArchiveTestLocation() { Id = A<int>(), Uuid = archiveTestLocationUuid };
            ExpectGetArchiveTestLocationReturns(organization.Id, archiveTestLocationUuid, (archiveTestLocation, true));
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectRemoveAllArchivePeriodsReturns(itSystemUsage.Id, new List<ArchivePeriod>());

            var failure = A<OperationFailure>();
            _itSystemUsageServiceMock.Setup(x => x.AddArchivePeriod(itSystemUsage.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new OperationError(failure));

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(failure, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Can_Create_With_Roles()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();

            var input = CreateSystemUsageUpdateParametersWithData(new List<UserRolePair>()
            {
                CreateUserRolePair(roleUuid, userUuid)
            });

            var right = CreateRight(itSystemUsage, roleUuid, userUuid);

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectRoleAssignmentReturns(itSystemUsage, roleUuid, userUuid, right);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Can_Create_With_No_Roles()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var input = CreateSystemUsageUpdateParametersWithoutData();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_Role_Assignment_Fails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();

            var input = CreateSystemUsageUpdateParametersWithData(new List<UserRolePair>()
            {
                CreateUserRolePair(roleUuid, userUuid)
            });
            var error = A<OperationFailure>();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectRoleAssignmentReturns(itSystemUsage, roleUuid, userUuid, new OperationError(error));

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailure(createResult, error, transactionMock);
        }

        [Fact]
        public void Can_Update_Roles_To_Remove_Them()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var newRight = CreateRight(itSystemUsage, A<Guid>(), A<Guid>());
            var newUserRolePair = CreateUserRolePair(newRight.Role.Uuid, newRight.User.Uuid);

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectRoleAssignmentReturns(itSystemUsage, newRight.Role.Uuid, newRight.User.Uuid, newRight);

            var rightToRemove1 = CreateRight(itSystemUsage, A<Guid>(), A<Guid>());
            var rightToKeep = CreateRight(itSystemUsage, A<Guid>(), A<Guid>());
            var userRolePairToKeep = CreateUserRolePair(rightToKeep.Role.Uuid, rightToKeep.User.Uuid);
            itSystemUsage.Rights.Add(rightToRemove1);
            itSystemUsage.Rights.Add(rightToKeep);

            var roleInput = CreateSystemUsageUpdateParametersWithData(new List<UserRolePair>()
            {
                newUserRolePair, userRolePairToKeep
            });

            ExpectRoleRemoveReturns(itSystemUsage, rightToRemove1.Role.Uuid, rightToRemove1.User.Uuid, rightToRemove1);
            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);

            //Act
            var updateResult = _sut.Update(itSystemUsage.Uuid, roleInput);

            //Assert
            Assert.True(updateResult.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertRemoveRoleCalledOnce(itSystemUsage, rightToRemove1.Role.Uuid, rightToRemove1.User.Uuid);
            AssertAssignRoleCalledOnce(itSystemUsage, newRight.Role.Uuid, newRight.User.Uuid);
        }

        [Fact]
        public void Can_Create_With_GDPR()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            var sensitiveDataTypeUuids = Many<Guid>().Distinct().ToList();
            var registerTypeUuids = Many<Guid>().Distinct().ToList();

            ExpectUpdateSensitiveDataTypesReturns(itSystemUsage, sensitiveDataTypeUuids, sensitiveDataTypeUuids.Select(uuid => new SensitivePersonalDataType { Uuid = uuid, Id = A<int>() }).ToList());
            ExpectUpdateRegisterTypesReturns(itSystemUsage, registerTypeUuids, registerTypeUuids.Select(uuid => new RegisterType { Uuid = uuid, Id = A<int>() }).ToList());

            var purpose = A<string>();
            var businessCritical = A<DataOptions?>();
            var hostedAt = A<HostedAt?>();
            var directoryDoc = A<NamedLink>();
            var sensitiveDataLevels = Many<SensitiveDataLevel>().Distinct().ToList();
            var technicalPrecautionsInPlace = A<DataOptions?>();
            var technicalPrecautions = Many<TechnicalPrecaution>().Distinct().ToList();
            var technicalPrecautionsDocumentation = A<NamedLink>();
            var userSupervision = A<DataOptions?>();
            var supervisionDate = A<DateTime?>();
            var supervisionDoc = A<NamedLink>();
            var riskAssessmentConducted = A<DataOptions?>();
            var riskAssessmentDate = A<DateTime?>();
            var riskAssessmentDoc = A<NamedLink>();
            var riskAssessmentNotes = A<string>();
            var riskAssessmentResult = A<RiskLevel?>();
            var dpiaConducted = A<DataOptions?>();
            var dpiaDate = A<DateTime?>();
            var dpiaDoc = A<NamedLink>();
            var retentionPeriod = A<DataOptions?>();
            var nextEvaluationDate = A<DateTime?>();
            var evaluationFrequency = A<int?>();
            var gdprInput = new UpdatedSystemUsageGDPRProperties
            {
                Purpose = purpose.AsChangedValue(),
                BusinessCritical = businessCritical.AsChangedValue(),
                HostedAt = hostedAt.AsChangedValue(),
                DirectoryDocumentation = directoryDoc.FromNullable().AsChangedValue(),
                DataSensitivityLevels = sensitiveDataLevels.FromNullable<IEnumerable<SensitiveDataLevel>>().AsChangedValue(),
                SensitivePersonDataUuids = sensitiveDataTypeUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue(),
                RegisteredDataCategoryUuids = registerTypeUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue(),
                TechnicalPrecautionsInPlace = technicalPrecautionsInPlace.AsChangedValue(),
                TechnicalPrecautionsApplied = technicalPrecautions.FromNullable<IEnumerable<TechnicalPrecaution>>().AsChangedValue(),
                TechnicalPrecautionsDocumentation = technicalPrecautionsDocumentation.FromNullable().AsChangedValue(),
                UserSupervision = userSupervision.AsChangedValue(),
                UserSupervisionDate = supervisionDate.AsChangedValue(),
                UserSupervisionDocumentation = supervisionDoc.FromNullable().AsChangedValue(),
                RiskAssessmentConducted = riskAssessmentConducted.AsChangedValue(),
                RiskAssessmentConductedDate = riskAssessmentDate.AsChangedValue(),
                RiskAssessmentDocumentation = riskAssessmentDoc.FromNullable().AsChangedValue(),
                RiskAssessmentNotes = riskAssessmentNotes.AsChangedValue(),
                RiskAssessmentResult = riskAssessmentResult.AsChangedValue(),
                DPIAConducted = dpiaConducted.AsChangedValue(),
                DPIADate = dpiaDate.AsChangedValue(),
                DPIADocumentation = dpiaDoc.FromNullable().AsChangedValue(),
                RetentionPeriodDefined = retentionPeriod.AsChangedValue(),
                NextDataRetentionEvaluationDate = nextEvaluationDate.AsChangedValue(),
                DataRetentionEvaluationFrequencyInMonths = evaluationFrequency.AsChangedValue()
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters
            {
                GDPR = gdprInput
            }));

            //Assert
            Assert.True(createResult.Ok);
            Assert.Same(itSystemUsage, createResult.Value);
            AssertTransactionCommitted(transactionMock);
            Assert.Equal(purpose, itSystemUsage.GeneralPurpose);
            Assert.Equal(businessCritical, itSystemUsage.isBusinessCritical);
            Assert.Equal(hostedAt, itSystemUsage.HostedAt);
            AssertLink(directoryDoc, itSystemUsage.LinkToDirectoryUrlName, itSystemUsage.LinkToDirectoryUrl);
            Assert.Equal(sensitiveDataLevels.OrderBy(x => x), itSystemUsage.SensitiveDataLevels.Select(x => x.SensitivityDataLevel).OrderBy(x => x));
            _sensitiveDataOptionsService.Verify(x => x.UpdateAssignedOptions(itSystemUsage, sensitiveDataTypeUuids), Times.Once);
            _registerTypeOptionsService.Verify(x => x.UpdateAssignedOptions(itSystemUsage, registerTypeUuids), Times.Once);
            Assert.Equal(technicalPrecautionsInPlace, itSystemUsage.precautions);
            Assert.Equal(technicalPrecautions.OrderBy(x => x), itSystemUsage.GetTechnicalPrecautions().OrderBy(x => x));
            AssertLink(technicalPrecautionsDocumentation, itSystemUsage.TechnicalSupervisionDocumentationUrlName, itSystemUsage.TechnicalSupervisionDocumentationUrl);
            Assert.Equal(userSupervision, itSystemUsage.UserSupervision);
            Assert.Equal(supervisionDate, itSystemUsage.UserSupervisionDate);
            AssertLink(supervisionDoc, itSystemUsage.UserSupervisionDocumentationUrlName, itSystemUsage.UserSupervisionDocumentationUrl);
            Assert.Equal(riskAssessmentConducted, itSystemUsage.riskAssessment);
            Assert.Equal(riskAssessmentDate, itSystemUsage.riskAssesmentDate);
            AssertLink(riskAssessmentDoc, itSystemUsage.RiskSupervisionDocumentationUrlName, itSystemUsage.RiskSupervisionDocumentationUrl);
            Assert.Equal(riskAssessmentNotes, itSystemUsage.noteRisks);
            Assert.Equal(riskAssessmentResult, itSystemUsage.preriskAssessment);
            Assert.Equal(dpiaConducted, itSystemUsage.DPIA);
            Assert.Equal(dpiaDate, itSystemUsage.DPIADateFor);
            AssertLink(dpiaDoc, itSystemUsage.DPIASupervisionDocumentationUrlName, itSystemUsage.DPIASupervisionDocumentationUrl);
            Assert.Equal(retentionPeriod, itSystemUsage.answeringDataDPIA);
            Assert.Equal(nextEvaluationDate, itSystemUsage.DPIAdeleteDate);
            Assert.Equal(evaluationFrequency, itSystemUsage.numberDPIA);
        }

        [Fact]
        public void Cannot_Create_With_GDPR_If_SensitiveDataLevelsAreNotUnique()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var sensitiveDataLevels = Many<SensitiveDataLevel>().Distinct().ToList();
            sensitiveDataLevels.Add(sensitiveDataLevels.First()); //Add duplicate
            var gdprInput = new UpdatedSystemUsageGDPRProperties
            {
                DataSensitivityLevels = sensitiveDataLevels.FromNullable<IEnumerable<SensitiveDataLevel>>().AsChangedValue(),
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters
            {
                GDPR = gdprInput
            }));

            //Assert
            AssertFailureWithErrorMessageContent(createResult, OperationFailure.BadInput, "Duplicate sensitivity levels are not allowed", transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_GDPR_If_AppliedTechnicalPrecutionsAreNotUnique()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var precautions = Many<TechnicalPrecaution>().Distinct().ToList();
            precautions.Add(precautions.First()); //Add duplicate
            var gdprInput = new UpdatedSystemUsageGDPRProperties
            {
                TechnicalPrecautionsApplied = precautions.FromNullable<IEnumerable<TechnicalPrecaution>>().AsChangedValue(),
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters
            {
                GDPR = gdprInput
            }));

            //Assert
            AssertFailureWithErrorMessageContent(createResult, OperationFailure.BadInput, "Duplicates are not allowed in technical precautions", transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_GDPR_If_RegisterTypeUpdatesFails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var ids = Many<Guid>().ToList();
            var operationError = A<OperationError>();
            ExpectUpdateRegisterTypesReturns(itSystemUsage, ids, operationError);

            var gdprInput = new UpdatedSystemUsageGDPRProperties
            {
                RegisteredDataCategoryUuids = ids.FromNullable<IEnumerable<Guid>>().AsChangedValue(),
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters
            {
                GDPR = gdprInput
            }));

            //Assert
            AssertFailureWithExpectedOperationError(createResult, operationError, transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_GDPR_If_SensitivePersonDataUpdateFails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            var ids = Many<Guid>().ToList();
            var operationError = A<OperationError>();
            ExpectUpdateSensitiveDataTypesReturns(itSystemUsage, ids, operationError);

            var gdprInput = new UpdatedSystemUsageGDPRProperties
            {
                SensitivePersonDataUuids = ids.FromNullable<IEnumerable<Guid>>().AsChangedValue(),
            };

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters
            {
                GDPR = gdprInput
            }));

            //Assert
            AssertFailureWithExpectedOperationError(createResult, operationError, transactionMock);
        }

        [Fact]
        public void Can_Delete_SystemUsage()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var systemUsageId = A<int>();
            var systemToBeDeleted = new ItSystemUsage() { Id = systemUsageId };
            _itSystemUsageServiceMock.Setup(x => x.GetByUuid(systemUsageUuid)).Returns(systemToBeDeleted);
            _itSystemUsageServiceMock.Setup(x => x.Delete(systemUsageId)).Returns(systemToBeDeleted);

            //Act
            var deleteResult = _sut.Delete(systemUsageUuid);

            //Assert
            Assert.True(deleteResult.IsNone);
        }

        [Fact]
        public void Cannot_Delete_SystemUsage_Returns_Not_Found_If_Not_Exists()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            _itSystemUsageServiceMock.Setup(x => x.GetByUuid(systemUsageUuid)).Returns(new OperationError(OperationFailure.NotFound));

            //Act
            var deleteResult = _sut.Delete(systemUsageUuid);

            //Assert
            Assert.True(deleteResult.HasValue);
            Assert.Equal(OperationFailure.NotFound, deleteResult.Value.FailureType);
        }

        [Fact]
        public void Cannot_Delete_SystemUsage_Returns_Error_If_Delete_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var systemUsageId = A<int>();
            var systemToBeDeleted = new ItSystemUsage() { Id = systemUsageId };
            var error = new OperationError(A<OperationFailure>());
            _itSystemUsageServiceMock.Setup(x => x.GetByUuid(systemUsageUuid)).Returns(systemToBeDeleted);
            _itSystemUsageServiceMock.Setup(x => x.Delete(systemUsageId)).Returns(error);

            //Act
            var deleteResult = _sut.Delete(systemUsageUuid);

            //Assert
            Assert.True(deleteResult.HasValue);
            Assert.Equal(error.FailureType, deleteResult.Value.FailureType);
        }

        private static void AssertFailureWithExpectedOperationError(Result<ItSystemUsage, OperationError> createResult, OperationError operationError,
            Mock<IDatabaseTransaction> transactionMock)
        {
            AssertFailureWithErrorMessageContent(createResult, operationError.FailureType, operationError.Message.GetValueOrEmptyString(), transactionMock);
        }

        private static void AssertFailureWithErrorMessageContent(Result<ItSystemUsage, OperationError> createResult, OperationFailure expectedFailure, string expectedContent,
            Mock<IDatabaseTransaction> transactionMock)
        {
            AssertFailure(createResult, expectedFailure, transactionMock);
            Assert.Contains(expectedContent, createResult.Error.Message.GetValueOrDefault());
        }

        private static void AssertFailure(Result<ItSystemUsage, OperationError> createResult, OperationFailure expectedFailure, Mock<IDatabaseTransaction> transactionMock)
        {
            Assert.True(createResult.Failed);
            Assert.Equal(expectedFailure, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        private static void AssertLink(NamedLink expectedLink, string actualName, string actualUrl)
        {
            Assert.Equal(expectedLink.Name, actualName);
            Assert.Equal(expectedLink.Url, actualUrl);
        }

        private void ExpectUpdateSensitiveDataTypesReturns(ItSystemUsage itSystemUsage,
            IReadOnlyCollection<Guid> sensitiveDataTypeUuids,
            Result<IEnumerable<SensitivePersonalDataType>, OperationError> result)
        {
            _sensitiveDataOptionsService.Setup(x => x.UpdateAssignedOptions(itSystemUsage, sensitiveDataTypeUuids)).Returns(result);
        }

        private void ExpectUpdateRegisterTypesReturns(ItSystemUsage itSystemUsage,
            IReadOnlyCollection<Guid> optionUuids,
            Result<IEnumerable<RegisterType>, OperationError> result)
        {
            _registerTypeOptionsService.Setup(x => x.UpdateAssignedOptions(itSystemUsage, optionUuids)).Returns(result);
        }


        private void AssertEmptyArchivingParameters(ItSystemUsage actual)
        {
            Assert.Null(actual.ArchiveDuty);
            Assert.Null(actual.ArchiveType);
            Assert.Null(actual.ArchiveLocation);
            Assert.Null(actual.ArchiveTestLocation);
            Assert.Equal("", actual.ArchiveSupplier);
            Assert.Null(actual.SupplierId);
            Assert.Null(actual.ArchiveFromSystem);
            Assert.Null(actual.Registertype);
            Assert.Null(actual.ArchiveFreq);
            Assert.Equal("", actual.ArchiveNotes);
            _itSystemUsageServiceMock.Verify(x => x.RemoveAllArchivePeriods(actual.Id), Times.Once);
            _itSystemUsageServiceMock.Verify(x => x.AddArchivePeriod(actual.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        private void AssertArchivingParameters(UpdatedSystemUsageArchivingParameters expected, string expectedSupplierName, int expectedSupplierId, ItSystemUsage actual)
        {
            Assert.Equal(expected.ArchiveDuty.NewValue, actual.ArchiveDuty);
            Assert.Equal(expected.ArchiveTypeUuid.NewValue.Value, actual.ArchiveType.Uuid);
            Assert.Equal(expected.ArchiveLocationUuid.NewValue.Value, actual.ArchiveLocation.Uuid);
            Assert.Equal(expected.ArchiveTestLocationUuid.NewValue.Value, actual.ArchiveTestLocation.Uuid);
            Assert.Equal(expectedSupplierName, actual.ArchiveSupplier);
            Assert.Equal(expectedSupplierId, actual.SupplierId);
            Assert.Equal(expected.ArchiveActive.NewValue, actual.ArchiveFromSystem);
            Assert.Equal(expected.ArchiveDocumentBearing.NewValue, actual.Registertype);
            Assert.Equal(expected.ArchiveFrequencyInMonths.NewValue, actual.ArchiveFreq);
            Assert.Equal(expected.ArchiveNotes.NewValue, actual.ArchiveNotes);
            _itSystemUsageServiceMock.Verify(x => x.RemoveAllArchivePeriods(actual.Id), Times.Once);
            _itSystemUsageServiceMock.Verify(x => x.AddArchivePeriod(actual.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(expected.ArchiveJournalPeriods.NewValue.Value.Count()));
        }

        private static UpdatedSystemUsageArchivingParameters CreateEmptySystemUsageArchivingParameters()
        {
            return new UpdatedSystemUsageArchivingParameters()
            {
                ArchiveDuty = new ChangedValue<ArchiveDutyTypes?>(null),
                ArchiveTypeUuid = Maybe<Guid>.None.AsChangedValue(),
                ArchiveLocationUuid = Maybe<Guid>.None.AsChangedValue(),
                ArchiveTestLocationUuid = Maybe<Guid>.None.AsChangedValue(),
                ArchiveSupplierOrganizationUuid = Maybe<Guid>.None.AsChangedValue(),
                ArchiveActive = new ChangedValue<bool?>(null),
                ArchiveDocumentBearing = new ChangedValue<bool?>(null),
                ArchiveFrequencyInMonths = new ChangedValue<int?>(null),
                ArchiveNotes = "".AsChangedValue(),
                ArchiveJournalPeriods = Maybe<IEnumerable<SystemUsageJournalPeriod>>.None.AsChangedValue()
            };
        }

        private UpdatedSystemUsageArchivingParameters CreateSystemUsageArchivingParameters(Maybe<Guid> archiveTypeUuid, Maybe<Guid> archiveLocationUuid, Maybe<Guid> archiveTestLocationUuid, Maybe<Guid> archiveSupplierUuid)
        {
            return new UpdatedSystemUsageArchivingParameters()
            {
                ArchiveDuty = A<ArchiveDutyTypes?>().AsChangedValue(),
                ArchiveTypeUuid = archiveTypeUuid.AsChangedValue(),
                ArchiveLocationUuid = archiveLocationUuid.AsChangedValue(),
                ArchiveTestLocationUuid = archiveTestLocationUuid.AsChangedValue(),
                ArchiveSupplierOrganizationUuid = archiveSupplierUuid.AsChangedValue(),
                ArchiveActive = A<bool?>().AsChangedValue(),
                ArchiveDocumentBearing = A<bool?>().AsChangedValue(),
                ArchiveFrequencyInMonths = new ChangedValue<int?>(A<int>()),
                ArchiveNotes = A<string>().AsChangedValue(),
                ArchiveJournalPeriods = Many<SystemUsageJournalPeriod>().ToList().FromNullable<IEnumerable<SystemUsageJournalPeriod>>().AsChangedValue()
            };
        }

        private void ExpectAddArchivePeriodReturns(int itSystemUsageId, Result<ArchivePeriod, OperationError> addResult)
        {
            _itSystemUsageServiceMock.Setup(x => x.AddArchivePeriod(itSystemUsageId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(addResult);
        }

        private void ExpectRemoveAllArchivePeriodsReturns(int itSystemUsageId, IEnumerable<ArchivePeriod> removedPeriods)
        {
            _itSystemUsageServiceMock.Setup(x => x.RemoveAllArchivePeriods(itSystemUsageId)).Returns(removedPeriods.ToList());
        }

        private void ExpectGetArchiveTestLocationReturns(int organizationId, Guid archiveTestLocationUuid, Maybe<(ArchiveTestLocation, bool)> result)
        {
            _archiveTestLocationOptionsServiceMock.Setup(x => x.GetOptionByUuid(organizationId, archiveTestLocationUuid)).Returns(result);
        }

        private void ExpectGetArchiveLocationReturns(int organizationId, Guid archiveLocationUuid, Maybe<(ArchiveLocation, bool)> result)
        {
            _archiveLocationOptionsServiceMock.Setup(x => x.GetOptionByUuid(organizationId, archiveLocationUuid)).Returns(result);
        }

        private void ExpectGetArchiveTypeReturns(int organizationId, Guid archiveTypeUuid, Maybe<(ArchiveType, bool)> result)
        {
            _archiveTypeOptionsServiceMock.Setup(x => x.GetOptionByUuid(organizationId, archiveTypeUuid)).Returns(result);
        }

        private void ExpectAddExternalReferenceReturns(ItSystemUsage itSystemUsage, UpdatedExternalReferenceProperties externalReference, Result<ExternalReference, OperationError> value)
        {
            _referenceServiceMock
                .Setup(x => x.AddReference(itSystemUsage.Id, ReferenceRootType.SystemUsage, externalReference.Title, externalReference.DocumentId, externalReference.Url))
                .Returns(value);
        }

        private ExternalReference CreateExternalReference(UpdatedExternalReferenceProperties externalReference)
        {
            return new ExternalReference
            {
                Id = A<int>(),
                Title = externalReference.Title,
                ExternalReferenceId = externalReference.DocumentId,
                URL = externalReference.Url
            };
        }

        private SystemUsageUpdateParameters SetupKLEInputExpectations(IReadOnlyCollection<TaskRef> additionalTaskRefs, IReadOnlyCollection<TaskRef> tasksToRemove)
        {
            var input = new SystemUsageUpdateParameters
            {
                KLE = new UpdatedSystemUsageKLEDeviationParameters
                {
                    AddedKLEUuids = additionalTaskRefs.Select(x => x.Uuid).FromNullable().AsChangedValue(),
                    RemovedKLEUuids = tasksToRemove.Select(x => x.Uuid).FromNullable().AsChangedValue()
                }
            };

            foreach (var taskRef in additionalTaskRefs.Concat(tasksToRemove))
                ExpectGetKLEReturns(taskRef.Uuid, (Maybe<DateTime>.None, taskRef));

            return input;
        }

        private void ExpectGetKLEReturns(Guid uuid, Result<(Maybe<DateTime> updateReference, TaskRef kle), OperationError> result)
        {
            _kleServiceMock.Setup(x => x.GetKle(uuid)).Returns(result);
        }

        private List<TaskRef> CreateTaskRefs(int howMany)
        {
            return Many<Guid>(howMany).Select(CreateTaskRef).ToList();
        }

        private static TaskRef CreateTaskRef(Guid uuid)
        {
            return new TaskRef() { Uuid = uuid };
        }

        private void ExpectGetOrganizationUnitReturns(Guid orgUnitId, Result<OrganizationUnit, OperationError> organizationUnit)
        {
            _organizationServiceMock.Setup(x => x.GetOrganizationUnit(orgUnitId)).Returns(organizationUnit);
        }

        private OrganizationUnit CreateOrganizationUnit(Guid uuid, Organization organization)
        {
            return new OrganizationUnit
            {
                Uuid = uuid,
                Name = A<string>(),
                OrganizationId = organization.Id,
                Organization = organization
            };
        }

        private static SystemUsageUpdateParameters CreateSystemUsageUpdateParametersWithoutData()
        {
            return new SystemUsageUpdateParameters
            {
                Roles = new UpdatedSystemUsageRoles()
                {
                    UserRolePairs = Maybe<IEnumerable<UserRolePair>>.None.AsChangedValue()
                }.FromNullable()
            };
        }

        private static UserRolePair CreateUserRolePair(Guid roleUuid, Guid userUuid)
        {
            return new UserRolePair()
            {
                RoleUuid = roleUuid,
                UserUuid = userUuid
            };
        }

        private static SystemUsageUpdateParameters CreateSystemUsageUpdateParametersWithData(IEnumerable<UserRolePair> userRolePairs)
        {
            return new SystemUsageUpdateParameters()
            {
                Roles = new UpdatedSystemUsageRoles()
                {
                    UserRolePairs = userRolePairs.FromNullable().AsChangedValue()
                }
                    .FromNullable()
            };
        }

        private ItSystemRight CreateRight(ItSystemUsage itSystemUsage, Guid roleUuid, Guid userUuid)
        {
            return new ItSystemRight()
            {
                Object = itSystemUsage,
                Role = new ItSystemRole()
                {
                    Id = A<int>(),
                    Uuid = roleUuid
                },
                User = new User()
                {
                    Id = A<int>(),
                    Uuid = userUuid
                }
            };
        }

        private void AssertAssignRoleCalledOnce(ItSystemUsage itSystemUsage, Guid roleUuid, Guid userUuid)
        {
            _roleAssignmentService.Verify(x => x.AssignRole(itSystemUsage, roleUuid, userUuid), Times.Once);
        }

        private void AssertAssignRoleNeverCalled()
        {
            _roleAssignmentService.Verify(x => x.AssignRole(It.IsAny<ItSystemUsage>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        private void AssertRemoveRoleCalledOnce(ItSystemUsage itSystemUsage, Guid roleUuid, Guid userUuid)
        {
            _roleAssignmentService.Verify(x => x.RemoveRole(itSystemUsage, roleUuid, userUuid), Times.Once);
        }
        private void AssertRemoveRoleNeverCalled()
        {
            _roleAssignmentService.Verify(x => x.RemoveRole(It.IsAny<ItSystemUsage>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        private void ExpectRoleAssignmentReturns(ItSystemUsage itSystemUsage, Guid roleUuid, Guid userUuid, Result<ItSystemRight, OperationError> result)
        {
            _roleAssignmentService.Setup(x => x.AssignRole(itSystemUsage, roleUuid, userUuid)).Returns(result);
        }

        private void ExpectRoleRemoveReturns(ItSystemUsage itSystemUsage, Guid roleUuid, Guid userUuid, Result<ItSystemRight, OperationError> result)
        {
            _roleAssignmentService.Setup(x => x.RemoveRole(itSystemUsage, roleUuid, userUuid)).Returns(result);
        }

        private (Guid systemUuid, Guid organizationUuid, Mock<IDatabaseTransaction> transactionMock, Organization organization, ItSystem itSystem, ItSystemUsage itSystemUsage) CreateBasicTestVariables()
        {
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var organization = CreateOrganization();
            var itSystem = new ItSystem { Id = A<int>() };
            var itSystemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = organization.Id,
                ItSystem = itSystem
            };

            return (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage);
        }

        private void SetupBasicCreateThenUpdatePrerequisites(Guid organizationUuid, Organization organization, Guid systemUuid,
            ItSystem itSystem, ItSystemUsage itSystemUsage)
        {
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetSystemReturns(systemUuid, itSystem);
            ExpectCreateNewReturns(itSystem, organization, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);
        }

        private void ExpectGetProjectReturns(Guid projectUuid, Result<ItProject, OperationError> result)
        {
            _projectServiceMock.Setup(x => x.GetProject(projectUuid))
                .Returns(result);
        }

        private Organization CreateOrganization()
        {
            return new Organization { Id = A<int>() };
        }

        private static ItProject CreateItProject(Organization organization, Guid projectUuid)
        {
            return new ItProject { OrganizationId = organization.Id, Organization = organization, Uuid = projectUuid };
        }

        private void ExpectGetContractReturns(Guid newContractId, Result<ItContract, OperationError> result)
        {
            _contractServiceMock.Setup(x => x.GetContract(newContractId)).Returns(result);
        }

        private ItContractItSystemUsage CreateContractAssociation(Organization parentOrganization, ItContract newContract = null)
        {
            var itContract = newContract ?? CreateItContract(parentOrganization, Maybe<Guid>.None);
            return new ItContractItSystemUsage
            {
                ItContract = itContract,
                ItContractId = itContract.Id
            };
        }

        private ItContract CreateItContract(Organization parentOrganization, Maybe<Guid> uuid)
        {
            return new ItContract
            {
                Id = A<int>(),
                Uuid = uuid.Match(val => val, A<Guid>),
                Organization = parentOrganization,
                OrganizationId = parentOrganization.Id
            };
        }

        private void ExpectGetItSystemCategoryReturns(int organizationId, Guid dataClassificationId, Maybe<(ItSystemCategories, bool)> result)
        {
            _systemCategoriesOptionsServiceMock.Setup(x => x.GetOptionByUuid(organizationId, dataClassificationId)).Returns(result);
        }

        private static void AssertTransactionNotCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        private static void AssertTransactionCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit());
        }

        private void ExpectAllowModifyReturns(ItSystemUsage itSystemUsage, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(itSystemUsage)).Returns(value);
        }

        private void ExpectCreateNewReturns(ItSystem itSystem, Organization organization, Result<ItSystemUsage, OperationError> result)
        {
            _itSystemUsageServiceMock.Setup(x => x.CreateNew(itSystem.Id, organization.Id)).Returns(result);
        }

        private void ExpectGetSystemUsageReturns(Guid systemUuid, ItSystemUsage result)
        {
            _itSystemUsageServiceMock.Setup(x => x.GetByUuid(systemUuid)).Returns(result);
        }

        private void ExpectGetSystemReturns(Guid systemUuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(systemUuid)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid organizationUuid, Result<Organization, OperationError> organizationResult)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationUuid, null)).Returns(organizationResult);
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var trasactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(trasactionMock.Object);
            return trasactionMock;
        }
    }
}
