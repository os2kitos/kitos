using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Options;
using Core.DomainServices.Role;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Extensions;
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
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>> _roleAssignmentService;
        private readonly ItSystemUsageWriteService _sut;
        private readonly Mock<IKLEApplicationService> _kleServiceMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;
        private readonly Mock<IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>> _sensitiveDataOptionsService;
        private readonly Mock<IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage>> _registerTypeOptionsService;
        private readonly Mock<IGenericRepository<ItSystemUsageSensitiveDataLevel>> _sensitiveDataLevelRepository;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IItsystemUsageRelationsService> _systemUsageRelationServiceMock;
        private readonly Mock<IGenericRepository<ItSystemUsagePersonalData>> _personalDataOptionsRepository;

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
            _domainEventsMock = new Mock<IDomainEvents>();
            _kleServiceMock = new Mock<IKLEApplicationService>();
            _referenceServiceMock = new Mock<IReferenceService>();
            _roleAssignmentService = new Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>>();
            _sensitiveDataOptionsService = new Mock<IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>>();
            _registerTypeOptionsService = new Mock<IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage>>();
            _sensitiveDataLevelRepository = new Mock<IGenericRepository<ItSystemUsageSensitiveDataLevel>>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _systemUsageRelationServiceMock = new Mock<IItsystemUsageRelationsService>();
            _personalDataOptionsRepository = new Mock<IGenericRepository<ItSystemUsagePersonalData>>();
            _sut = new ItSystemUsageWriteService(_itSystemUsageServiceMock.Object, _transactionManagerMock.Object,
                _itSystemServiceMock.Object, _organizationServiceMock.Object, _authorizationContextMock.Object,
                _systemCategoriesOptionsServiceMock.Object, _contractServiceMock.Object,
                _kleServiceMock.Object, _referenceServiceMock.Object, _roleAssignmentService.Object,
                _sensitiveDataOptionsService.Object,
                _registerTypeOptionsService.Object,
                _sensitiveDataLevelRepository.Object,
                Mock.Of<IDatabaseControl>(), _domainEventsMock.Object, Mock.Of<ILogger>(),
                _archiveTypeOptionsServiceMock.Object, _archiveLocationOptionsServiceMock.Object,
                _archiveTestLocationOptionsServiceMock.Object,
                _systemUsageRelationServiceMock.Object,
                _identityResolverMock.Object,
                _personalDataOptionsRepository.Object);
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
        [InlineData(null, null, UserCount.UNDECIDED)]
        [InlineData(0, 9, UserCount.BELOWTEN)]
        [InlineData(10, 50, UserCount.TENTOFIFTY)]
        [InlineData(50, 100, UserCount.FIFTYTOHUNDRED)]
        [InlineData(100, null, UserCount.HUNDREDPLUS)]
        public void Can_Create_With_General_Data_With_All_Data_Defined(int? minimumNumberOfUsers, int? maxNumberOfUsers, UserCount expectedNumberOfUsers)
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var newContractId = A<Guid>();
            var newContract = CreateItContract(organization, newContractId);
            itSystemUsage.Contracts.Add(CreateContractAssociation(organization));
            itSystemUsage.Contracts.Add(CreateContractAssociation(organization, newContract));
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
                    DataClassificationUuid = Maybe<Guid>.Some(dataClassificationId).AsChangedValue(),
                    LifeCycleStatus = A<LifeCycleStatusType?>().AsChangedValue(),
                    ValidFrom = Maybe<DateTime>.Some(DateTime.Now).AsChangedValue(),
                    ValidTo = Maybe<DateTime>.Some(DateTime.Now.AddDays(Math.Abs(A<short>()))).AsChangedValue(),
                    MainContractUuid = Maybe<Guid>.Some(newContractId).AsChangedValue(),
                    NumberOfExpectedUsersInterval = minimumNumberOfUsers == null && maxNumberOfUsers == null ?
                        Maybe<(int lower, int? upperBound)>.None.AsChangedValue() :
                        Maybe<(int lower, int? upperBound)>.Some((minimumNumberOfUsers.GetValueOrDefault(), maxNumberOfUsers)).AsChangedValue(),
                }
            };

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            ExpectGetItSystemCategoryReturns(itSystemUsage.OrganizationId, dataClassificationId, (itSystemCategories, true));
            ExpectGetContractReturns(newContractId, newContract);

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
            Assert.Equal(generalProperties.DataClassificationUuid.NewValue.Value, itSystemUsage.ItSystemCategories.Uuid);
            Assert.Equal(generalProperties.LifeCycleStatus.NewValue, itSystemUsage.LifeCycleStatus);
            Assert.Equal(generalProperties.ValidFrom.NewValue.Value.Date, itSystemUsage.Concluded);
            Assert.Equal(generalProperties.ValidTo.NewValue.Value.Date, itSystemUsage.ExpirationDate);
            Assert.Equal(generalProperties.MainContractUuid.NewValue.Value, itSystemUsage.MainContract.ItContract.Uuid);
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

            itSystemUsage.LocalCallName = localCallName;
            itSystemUsage.LocalSystemId = localSystemId;
            itSystemUsage.Version = version;
            itSystemUsage.Note = note;
            itSystemUsage.Concluded = concluded;
            itSystemUsage.ExpirationDate = expirationDate;
            itSystemUsage.UserCount = userCount;
            itSystemUsage.ItSystemCategories = itSystemCategories;
            itSystemUsage.MainContract = itContractItSystemUsage;

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
            Assert.Equal(expirationDate, itSystemUsage.ExpirationDate);
            Assert.Equal(concluded, itSystemUsage.Concluded);
            Assert.Equal(userCount, itSystemUsage.UserCount);
            Assert.Equal(itSystemCategories, itSystemUsage.ItSystemCategories);
            Assert.Equal(itContractItSystemUsage, itSystemUsage.MainContract);
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
            AssertFailureWithErrorMessageContent(createResult, OperationFailure.BadInput, "Cannot Add KLE which is already present in the system context", transactionMock);

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
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();

            var input = new SystemUsageUpdateParameters
            {
                ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>()
            };

            ExpectBatchUpdateExternalReferencesReturns(itSystemUsage, externalReferences, Maybe<OperationError>.None);

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            //Act
            var result = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_ExternalReferences_If_BatchUpdate_Fails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();

            var input = new SystemUsageUpdateParameters
            {
                ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>()
            };
            var operationError = A<OperationError>();

            ExpectBatchUpdateExternalReferencesReturns(itSystemUsage, externalReferences, operationError);

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);

            //Act
            var result = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        private void ExpectBatchUpdateExternalReferencesReturns(ItSystemUsage systemUsage, IEnumerable<UpdatedExternalReferenceProperties> externalReferences, Maybe<OperationError> value)
        {
            _referenceServiceMock
                .Setup(x => x.UpdateExternalReferences(ReferenceRootType.SystemUsage, systemUsage.Id, externalReferences))
                .Returns(value);
        }

        [Fact]
        public void Can_Create_With_Archiving()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables(true);
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
            AssertArchivingParameters(archivingParameters, createResult.Value);
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
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables(true);

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
            AssertArchivingParameters(archivingParameters, createResult.Value);
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
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables(true);

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
            AssertArchivingParameters(archivingParameters, createResult.Value);
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
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables(true);

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
            AssertArchivingParameters(archivingParameters, createResult.Value);
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

            var userRolePairs = Many<UserRolePair>().ToList();

            var input = CreateSystemUsageUpdateParametersWithData(userRolePairs);

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            _roleAssignmentService
                .Setup(x => x.BatchUpdateRoles(itSystemUsage, It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments => MatchExpectedAssignments(assignments, userRolePairs))))
                .Returns(Maybe<OperationError>.None);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Can_Add_Role()
        {
            //Arrange
            var (_, _, transactionMock, _, _, itSystemUsage) = CreateBasicTestVariables();
            var existingAssignment = A<UserRolePair>();
            itSystemUsage.Rights.Add(new ItSystemRight { Role = new ItSystemRole { Uuid = existingAssignment.RoleUuid }, User = new User { Uuid = existingAssignment.UserUuid } });
            var newAssignment = A<UserRolePair>();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            _roleAssignmentService
                .Setup(x => x.BatchUpdateRoles(
                        itSystemUsage,
                        It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments =>
                            MatchExpectedAssignments(assignments, new[] { existingAssignment, newAssignment }.ToList()))
                    )
                )
                .Returns(Maybe<OperationError>.None);

            //Act
            var createResult = _sut.AddRole(itSystemUsage.Uuid, newAssignment);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Add_Duplicate_Role_Assignment()
        {
            //Arrange
            var (_, _, transactionMock, _, _, itSystemUsage) = CreateBasicTestVariables();
            var existingAssignment = A<UserRolePair>();
            itSystemUsage.Rights.Add(new ItSystemRight { Role = new ItSystemRole { Uuid = existingAssignment.RoleUuid }, User = new User { Uuid = existingAssignment.UserUuid } });

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            //Act
            var createResult = _sut.AddRole(itSystemUsage.Uuid, existingAssignment);

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.Conflict, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Can_Remove_Role()
        {
            //Arrange
            var (_, _, transactionMock, _, _, itSystemUsage) = CreateBasicTestVariables();
            var existingAssignment1 = A<UserRolePair>();
            var existingAssignment2 = A<UserRolePair>();
            itSystemUsage.Rights.Add(new ItSystemRight { Role = new ItSystemRole { Uuid = existingAssignment1.RoleUuid }, User = new User { Uuid = existingAssignment1.UserUuid } });
            itSystemUsage.Rights.Add(new ItSystemRight { Role = new ItSystemRole { Uuid = existingAssignment2.RoleUuid }, User = new User { Uuid = existingAssignment2.UserUuid } });

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            _roleAssignmentService
                .Setup(x => x.BatchUpdateRoles(
                        itSystemUsage,
                        It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments =>
                            MatchExpectedAssignments(assignments, new[] { existingAssignment1 }.ToList()))
                    )
                )
                .Returns(Maybe<OperationError>.None);

            //Act
            var createResult = _sut.RemoveRole(itSystemUsage.Uuid, existingAssignment2);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Remove_Role_If_Not_Assigned()
        {
            //Arrange
            var (_, _, transactionMock, _, _, itSystemUsage) = CreateBasicTestVariables();
            var existingAssignment1 = A<UserRolePair>();
            var assignmentThatDoesNotExist = A<UserRolePair>();
            itSystemUsage.Rights.Add(new ItSystemRight { Role = new ItSystemRole { Uuid = existingAssignment1.RoleUuid }, User = new User { Uuid = existingAssignment1.UserUuid } });

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            //Act
            var createResult = _sut.RemoveRole(itSystemUsage.Uuid, assignmentThatDoesNotExist);

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_BatchUpdateRoles_Fails()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var userRolePairs = Many<UserRolePair>().ToList();

            var input = CreateSystemUsageUpdateParametersWithData(userRolePairs);

            SetupBasicCreateThenUpdatePrerequisites(organizationUuid, organization, systemUuid, itSystem, itSystemUsage);
            var updateError = A<OperationError>();
            _roleAssignmentService
                .Setup(x => x.BatchUpdateRoles(itSystemUsage, It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments => MatchExpectedAssignments(assignments, userRolePairs))))
                .Returns(updateError);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, input));

            //Assert
            AssertFailureWithExpectedOperationError(createResult, updateError, transactionMock);
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
            DataOptions? userSupervision = DataOptions.YES;
            var supervisionDate = A<DateTime?>();
            var supervisionDoc = A<NamedLink>();
            DataOptions? riskAssessmentConducted = DataOptions.YES;
            var riskAssessmentDate = A<DateTime?>();
            var riskAssessmentDoc = A<NamedLink>();
            var riskAssessmentNotes = A<string>();
            var plannedRiskAssessmentDate = A<DateTime?>();
            var riskAssessmentResult = A<RiskLevel?>();
            DataOptions? dpiaConducted = DataOptions.YES;
            var dpiaDate = A<DateTime?>();
            var dpiaDoc = A<NamedLink>();
            var nextEvaluationDate = A<DateTime?>();
            var evaluationFrequency = A<int?>();
            DataOptions? retentionPeriodDefined = DataOptions.YES;
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
                PlannedRiskAssessmentDate = plannedRiskAssessmentDate.AsChangedValue(),
                RiskAssessmentResult = riskAssessmentResult.AsChangedValue(),
                DPIAConducted = dpiaConducted.AsChangedValue(),
                DPIADate = dpiaDate.AsChangedValue(),
                DPIADocumentation = dpiaDoc.FromNullable().AsChangedValue(),
                RetentionPeriodDefined = retentionPeriodDefined.AsChangedValue(),
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
            Assert.NotNull(itSystemUsage.UserSupervision);
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
            Assert.Equal(retentionPeriodDefined, itSystemUsage.answeringDataDPIA);
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
            DataOptions? technicalPrecautionsInPlace = DataOptions.YES;
            var gdprInput = new UpdatedSystemUsageGDPRProperties
            {
                TechnicalPrecautionsInPlace = technicalPrecautionsInPlace.AsChangedValue(),
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

            ExpectGetReadableItSystemUsageByUuidReturns(systemUsageUuid, systemToBeDeleted);
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
            ExpectGetReadableItSystemUsageByUuidReturns(systemUsageUuid, new OperationError(OperationFailure.NotFound));

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
            ExpectGetReadableItSystemUsageByUuidReturns(systemUsageUuid, systemToBeDeleted);
            _itSystemUsageServiceMock.Setup(x => x.Delete(systemUsageId)).Returns(error);

            //Act
            var deleteResult = _sut.Delete(systemUsageUuid);

            //Assert
            Assert.True(deleteResult.HasValue);
            Assert.Equal(error.FailureType, deleteResult.Value.FailureType);
        }

        [Fact]
        public void Cannot_DeleteBySystemAndOrgUuids_SystemUsage_If_GetSystem_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var error = A<OperationError>();

            ExpectGetSystemReturns(systemUuid, error);
            //Act
            var resultError = _sut.DeleteByItSystemAndOrganizationUuids(systemUuid, organizationUuid);

            //Assert
            Assert.True(resultError.HasValue);
            Assert.Equal(error.FailureType, resultError.Value.FailureType);
        }

        [Fact]
        public void Cannot_DeleteBySystemAndOrgUuids_SystemUsage_If_GetOrganization_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var error = A<OperationError>();

            ExpectGetSystemReturns(systemUuid, new ItSystem { Uuid = systemUuid, Id = A<int>() });
            ExpectGetOrganizationReturns(organizationUuid, error);
            //Act
            var resultError = _sut.DeleteByItSystemAndOrganizationUuids(systemUuid, organizationUuid);

            //Assert
            Assert.True(resultError.HasValue);
            Assert.Equal(error.FailureType, resultError.Value.FailureType);
        }

        [Fact]
        public void Cannot_DeleteBySystemAndOrgUuids_SystemUsage_If_Delete_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var usageUuid = A<Guid>();
            var system = new ItSystem { Uuid = systemUuid, Id = A<int>() };
            var organization = new Organization { Uuid = organizationUuid, Id = A<int>() };
            var usage = new ItSystemUsage { Uuid = usageUuid, Id = A<int>() };
            var error = A<OperationError>();

            ExpectGetSystemReturns(systemUuid, system);
            ExpectGetOrganizationReturns(organizationUuid, organization);
            _itSystemUsageServiceMock.Setup(x => x.GetByOrganizationAndSystemId(organization.Id, system.Id)).Returns(usage);
            _itSystemUsageServiceMock.Setup(x => x.Delete(usage.Id)).Returns(error);

            //Act
            var resultError = _sut.DeleteByItSystemAndOrganizationUuids(systemUuid, organizationUuid);

            //Assert
            Assert.True(resultError.HasValue);
            Assert.Equal(error.FailureType, resultError.Value.FailureType);
        }

        [Fact]
        public void Can_DeleteBySystemAndOrgUuids_SystemUsage()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var usageUuid = A<Guid>();
            var system = new ItSystem { Uuid = systemUuid, Id = A<int>() };
            var organization = new Organization { Uuid = organizationUuid, Id = A<int>() };
            var usage = new ItSystemUsage { Uuid = usageUuid, Id = A<int>() };

            ExpectGetSystemReturns(systemUuid, system);
            ExpectGetOrganizationReturns(organizationUuid, organization);
            _itSystemUsageServiceMock.Setup(x => x.GetByOrganizationAndSystemId(organization.Id, system.Id)).Returns(usage);
            _itSystemUsageServiceMock.Setup(x => x.Delete(usage.Id)).Returns(usage);

            //Act
            var resultError = _sut.DeleteByItSystemAndOrganizationUuids(systemUuid, organizationUuid);

            //Assert
            Assert.True(resultError.IsNone);
        }

        [Fact]
        public void Can_Update_All_On_Empty_ItSystemUsage()
        {
            //Arrange
            var (_, _, transactionMock, _, _, itSystemUsage) = CreateBasicTestVariables();

            var updateParameters = CreateSystemUsageUpdateParametersWithSimpleParametersAdded();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            //Act
            var updateResult = _sut.Update(itSystemUsage.Uuid, updateParameters);

            //Assert
            Assert.True(updateResult.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertSystemUsageUpdateParametersWithSimpleParametersAdded(updateParameters, updateResult.Value);
        }

        [Fact]
        public void Can_Update_With_Specific_Journal_Period_Update()
        {
            //Arrange
            var (_, _, transactionMock, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var archivingParameters = CreateUpdatedSystemUsageArchivingParameters();

            var updatedUuid = Guid.NewGuid();
            var periodUpdateParams = archivingParameters.ArchiveJournalPeriods.NewValue.Value.RandomItem();
            periodUpdateParams.Uuid = updatedUuid;

            Configure(f => f.Register(() => new ArchivePeriod
            {
                Approved = A<bool>(),
                UniqueArchiveId = A<string>(),
                StartDate = A<DateTime>(),
                EndDate = A<DateTime>()
            }));
            itSystemUsage.ArchivePeriods.Add(A<ArchivePeriod>());
            itSystemUsage.ArchivePeriods.Add(A<ArchivePeriod>());

            var periodsBeforeUpdate = itSystemUsage.ArchivePeriods.ToList();
            var periodToBeUpdated = periodsBeforeUpdate.RandomItem();
            periodToBeUpdated.Uuid = updatedUuid;
            var periodToBeReplaced = periodsBeforeUpdate.First(p => p != periodToBeUpdated);

            ExpectAddArchivePeriodReturns(itSystemUsage.Id, Result<ArchivePeriod, OperationError>.Success(A<ArchivePeriod>()));
            ExpectRemoveArchivePeriodReturns(itSystemUsage.Id, periodToBeReplaced.Uuid, periodToBeReplaced);
            ExpectUpdateArchivePeriodReturns(itSystemUsage.Id, updatedUuid, periodUpdateParams.StartDate, periodUpdateParams.EndDate, periodUpdateParams.ArchiveId, periodUpdateParams.Approved, periodToBeUpdated);

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var updateResult = _sut.Update(itSystemUsage.Uuid, input);

            //Assert
            Assert.True(updateResult.Ok);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Cannot_Update_With_Specific_Updates_If_Duplicates_Are_Detected()
        {
            //Arrange
            var (_, _, transactionMock, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var archivingParameters = CreateUpdatedSystemUsageArchivingParameters();

            var updatedUuid = Guid.NewGuid();
            var duplicatePeriodUpdates = archivingParameters
                .ArchiveJournalPeriods
                .NewValue
                .Value
                .RandomItems(2)
                .ToList();
            duplicatePeriodUpdates.ForEach(x => x.Uuid = updatedUuid);

            var input = new SystemUsageUpdateParameters
            {
                Archiving = archivingParameters
            };

            //Act
            var updateResult = _sut.Update(itSystemUsage.Uuid, input);

            //Assert
            Assert.True(updateResult.Failed);
            Assert.Equal(OperationFailure.BadInput, updateResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Can_Create_Journal_Period()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var input = A<SystemUsageJournalPeriodProperties>();
            ExpectAddArchivePeriodReturns(itSystemUsage.Id, input, new ArchivePeriod());

            //Act
            var result = _sut.CreateJournalPeriod(itSystemUsage.Uuid, input);

            //Assert
            Assert.True(result.Ok);
        }

        [Fact]
        public void Cannot_Create_Journal_Period_If_Removal_Fails()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var operationError = A<OperationError>();
            var input = A<SystemUsageJournalPeriodProperties>();
            ExpectAddArchivePeriodReturns(itSystemUsage.Id, input, operationError);

            //Act
            var result = _sut.CreateJournalPeriod(itSystemUsage.Uuid, input);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void Cannot_Create_Journal_Period_If_Not_Authorized()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, false);

            //Act
            var result = _sut.CreateJournalPeriod(itSystemUsage.Uuid, A<SystemUsageJournalPeriodProperties>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Delete_Journal_Period()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var periodUuid = A<Guid>();
            ExpectRemoveArchivePeriodReturns(itSystemUsage.Id, periodUuid, new ArchivePeriod());

            //Act
            var result = _sut.DeleteJournalPeriod(itSystemUsage.Uuid, periodUuid);

            //Assert
            Assert.True(result.Ok);
        }

        [Fact]
        public void Cannot_Delete_Journal_Period_If_Deletion_Fails()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var periodUuid = A<Guid>();
            var operationError = A<OperationError>();
            ExpectRemoveArchivePeriodReturns(itSystemUsage.Id, periodUuid, operationError);

            //Act
            var result = _sut.DeleteJournalPeriod(itSystemUsage.Uuid, periodUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void Cannot_Delete_Journal_Period_If_Not_Authorized()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, false);
            var periodUuid = A<Guid>();

            //Act
            var result = _sut.DeleteJournalPeriod(itSystemUsage.Uuid, periodUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_Archive_Period()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);
            var updatedProperties = A<SystemUsageJournalPeriodProperties>();

            var periodUuid = A<Guid>();
            ExpectUpdateArchivePeriodReturns(itSystemUsage.Id, periodUuid, updatedProperties.StartDate, updatedProperties.EndDate, updatedProperties.ArchiveId, updatedProperties.Approved, new ArchivePeriod());

            //Act
            var result = _sut.UpdateJournalPeriod(itSystemUsage.Uuid, periodUuid, updatedProperties);

            //Assert
            Assert.True(result.Ok);
        }

        [Fact]
        public void Cannot_Update_Journal_Period_If_Deletion_Fails()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);
            var updatedProperties = A<SystemUsageJournalPeriodProperties>();

            var periodUuid = A<Guid>();
            var operationError = A<OperationError>();
            ExpectUpdateArchivePeriodReturns(itSystemUsage.Id, periodUuid, updatedProperties.StartDate, updatedProperties.EndDate, updatedProperties.ArchiveId, updatedProperties.Approved, operationError);

            //Act
            var result = _sut.UpdateJournalPeriod(itSystemUsage.Uuid, periodUuid, updatedProperties);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void Cannot_Update_Journal_Period_If_Not_Authorized()
        {
            //Arrange
            var (_, _, _, _, _, itSystemUsage) = CreateBasicTestVariables();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, false);
            var updatedProperties = A<SystemUsageJournalPeriodProperties>();
            var periodUuid = A<Guid>();

            //Act
            var result = _sut.UpdateJournalPeriod(itSystemUsage.Uuid, periodUuid, updatedProperties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_All_On_Filled_Out_ItSystemUsage()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var updateParameters1 = CreateSystemUsageUpdateParametersWithSimpleParametersAdded();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var update1Result = _sut.Update(itSystemUsage.Uuid, updateParameters1);
            Assert.True(update1Result.Ok);

            var updateParameters2 = CreateSystemUsageUpdateParametersWithSimpleParametersAdded();

            //Act
            var update2Result = _sut.Update(itSystemUsage.Uuid, updateParameters2);

            //Assert
            Assert.True(update2Result.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertSystemUsageUpdateParametersWithSimpleParametersAdded(updateParameters2, update2Result.Value);
        }

        [Fact]
        public void Can_Update_All_To_Empty_On_Filled_Out_ItSystemUsage()
        {
            //Arrange
            var (systemUuid, organizationUuid, transactionMock, organization, itSystem, itSystemUsage) = CreateBasicTestVariables();

            var update1Parameters = CreateSystemUsageUpdateParametersWithSimpleParametersAdded();

            ExpectGetSystemUsageReturns(itSystemUsage.Uuid, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            var update1Result = _sut.Update(itSystemUsage.Uuid, update1Parameters);
            Assert.True(update1Result.Ok);

            var update2Parameters = CreateEmptySystemUsageUpdateParametersWithSimpleParametersAdded();

            //Act
            var update2Result = _sut.Update(itSystemUsage.Uuid, update2Parameters);

            //Assert
            Assert.True(update2Result.Ok);
            AssertTransactionCommitted(transactionMock);
            AssertSystemUsageUpdateParametersWithSimpleParametersAdded(update2Parameters, update2Result.Value, true);
        }

        [Fact]
        public void Can_Delete_SystemRelation()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var itSystemUsageRelationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var relationDbId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(itSystemUsageRelationUuid, relationDbId);
            ExpectRemoveRelationReturns(itSystemUsage.Id, relationDbId, new SystemRelation(itSystemUsage));

            //Act
            var error = _sut.DeleteSystemRelation(systemUsageUuid, itSystemUsageRelationUuid);

            //Assert
            Assert.True(error.IsNone);
        }

        [Fact]
        public void Cannot_Delete_SystemRelation_If_Removal_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var itSystemUsageRelationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var relationDbId = A<int>();
            var operationError = A<OperationError>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(itSystemUsageRelationUuid, relationDbId);
            ExpectRemoveRelationReturns(itSystemUsage.Id, relationDbId, operationError);

            //Act
            var error = _sut.DeleteSystemRelation(systemUsageUuid, itSystemUsageRelationUuid);

            //Assert
            Assert.True(error.HasValue);
            Assert.Same(operationError, error.Value);
        }

        [Fact]
        public void Cannot_Delete_SystemRelation_If_Relation_Id_Resolution_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var itSystemUsageRelationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(itSystemUsageRelationUuid, Maybe<int>.None);

            //Act
            var error = _sut.DeleteSystemRelation(systemUsageUuid, itSystemUsageRelationUuid);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_Delete_SystemRelation_If_SystemUsageRetrieval_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var itSystemUsageRelationUuid = A<Guid>();
            var operationError = A<OperationError>();
            ExpectGetSystemUsageReturns(systemUsageUuid, operationError);

            //Act
            var error = _sut.DeleteSystemRelation(systemUsageUuid, itSystemUsageRelationUuid);

            //Assert
            Assert.True(error.HasValue);
            Assert.Same(operationError, error.Value);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void Can_Create_SystemRelation(bool interfaceIdDefined, bool frequencyTypeDefined, bool contractDefined)
        {
            //Arrange - make sure uuids are defined for all
            var systemUsageUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), interfaceIdDefined ? A<Guid>() : null, contractDefined ? A<Guid>() : null, frequencyTypeDefined ? A<Guid>() : null, A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            var interfaceId = interfaceIdDefined ? A<int>() : (int?)null;
            var frequencyTypeId = frequencyTypeDefined ? A<int>() : (int?)null;
            var contractId = contractDefined ? A<int>() : (int?)null;
            var newRelation = new SystemRelation(itSystemUsage);
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItInterface>(systemRelationParameters.UsingInterfaceUuid, interfaceId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<RelationFrequencyType>(systemRelationParameters.RelationFrequencyUuid, frequencyTypeId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(systemRelationParameters.AssociatedContractUuid, contractId);
            ExpectAddSystemRelationReturns(itSystemUsage, toSystemUsageId, interfaceId, frequencyTypeId, contractId, systemRelationParameters.Description, systemRelationParameters.UrlReference, newRelation);

            //Act
            var result = _sut.CreateSystemRelation(systemUsageUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(newRelation, result.Value);
        }

        [Fact]
        public void Cannot_Create_SystemRelation_If_AddRelation_Fails()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => null));
            var systemUsageUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = A<SystemRelationParameters>();
            var toSystemUsageId = A<int>();
            var operationError = A<OperationError>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectAddSystemRelationReturns(itSystemUsage, toSystemUsageId, null, null, null, systemRelationParameters.Description, systemRelationParameters.UrlReference, operationError);

            //Act
            var result = _sut.CreateSystemRelation(systemUsageUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void Cannot_Create_SystemRelation_If_Resolve_InterfaceId_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), A<Guid>(), null, null, A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItInterface>(systemRelationParameters.UsingInterfaceUuid, Maybe<int>.None);

            //Act
            var result = _sut.CreateSystemRelation(systemUsageUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Create_SystemRelation_If_Resolve_ContractId_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), null, A<Guid>(), null, A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(systemRelationParameters.AssociatedContractUuid, Maybe<int>.None);

            //Act
            var result = _sut.CreateSystemRelation(systemUsageUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Create_SystemRelation_If_Resolve_FrequencyTypeId_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), null, null, A<Guid>(), A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<RelationFrequencyType>(systemRelationParameters.RelationFrequencyUuid, Maybe<int>.None);

            //Act
            var result = _sut.CreateSystemRelation(systemUsageUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Create_SystemRelation_If_Resolve_ToSystemUsageId_Fails()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => null));
            var systemUsageUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = A<SystemRelationParameters>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, Maybe<int>.None);

            //Act
            var result = _sut.CreateSystemRelation(systemUsageUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Create_SystemRelation_If_GetSystemUsage_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var systemRelationParameters = A<SystemRelationParameters>();
            var operationError = A<OperationError>();
            ExpectGetSystemUsageReturns(systemUsageUuid, operationError);

            //Act
            var result = _sut.CreateSystemRelation(systemUsageUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void Can_Update_SystemRelation(bool interfaceIdDefined, bool frequencyTypeDefined, bool contractDefined)
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), interfaceIdDefined ? A<Guid>() : null, contractDefined ? A<Guid>() : null, frequencyTypeDefined ? A<Guid>() : null, A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            var interfaceId = interfaceIdDefined ? A<int>() : (int?)null;
            var frequencyTypeId = frequencyTypeDefined ? A<int>() : (int?)null;
            var contractId = contractDefined ? A<int>() : (int?)null;
            var relationId = A<int>();
            var newRelation = new SystemRelation(itSystemUsage);
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(relationUuid, relationId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItInterface>(systemRelationParameters.UsingInterfaceUuid, interfaceId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<RelationFrequencyType>(systemRelationParameters.RelationFrequencyUuid, frequencyTypeId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(systemRelationParameters.AssociatedContractUuid, contractId);
            ExpectModifyRelationReturns(itSystemUsage, relationId, toSystemUsageId, interfaceId, frequencyTypeId, contractId, systemRelationParameters.Description, systemRelationParameters.UrlReference, newRelation);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(newRelation, result.Value);
        }

        [Fact]
        public void Cannot_Update_SystemRelation_If_Update_Fails()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => null));
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = A<SystemRelationParameters>();
            var toSystemUsageId = A<int>();
            var relationId = A<int>();
            var operationError = A<OperationError>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(relationUuid, relationId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectModifyRelationReturns(itSystemUsage, relationId, toSystemUsageId, null, null, null, systemRelationParameters.Description, systemRelationParameters.UrlReference, operationError);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void Cannot_Update_SystemRelation_If_Resolve_SystemRelationId_Fails()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => null));
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage() { Id = A<int>() };
            var systemRelationParameters = A<SystemRelationParameters>();
            var toSystemUsageId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(relationUuid, Maybe<int>.None);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Update_SystemRelation_If_Resolve_InterfaceId_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), A<Guid>(), null, null, A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(relationUuid, A<int>());
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItInterface>(systemRelationParameters.UsingInterfaceUuid, Maybe<int>.None);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Update_SystemRelation_If_Resolve_ContractId_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), null, A<Guid>(), null, A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(relationUuid, A<int>());
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(systemRelationParameters.AssociatedContractUuid, Maybe<int>.None);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Update_SystemRelation_If_Resolve_FrequencyTypeId_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage { Id = A<int>() };
            var systemRelationParameters = new SystemRelationParameters(A<Guid>(), null, null, A<Guid>(), A<string>(), A<string>());
            var toSystemUsageId = A<int>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, toSystemUsageId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<SystemRelation>(relationUuid, A<int>());
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<RelationFrequencyType>(systemRelationParameters.RelationFrequencyUuid, Maybe<int>.None);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Update_SystemRelation_If_ToSystemUsageId_Resolution_Fails()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => null));
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage { Id = A<int>() };
            var systemRelationParameters = A<SystemRelationParameters>();
            ExpectGetSystemUsageReturns(systemUsageUuid, itSystemUsage);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItSystemUsage>(systemRelationParameters.ToSystemUsageUuid, Maybe<int>.None);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Update_SystemRelation_If_SystemUsage_Resolution_Fails()
        {
            //Arrange
            var systemUsageUuid = A<Guid>();
            var relationUuid = A<Guid>();
            var systemRelationParameters = A<SystemRelationParameters>();
            var operationError = A<OperationError>();
            ExpectGetSystemUsageReturns(systemUsageUuid, operationError);

            //Act
            var result = _sut.UpdateSystemRelation(systemUsageUuid, relationUuid, systemRelationParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void Can_Add_ExternalReference()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var usage = new ItSystemUsage { Id = usageId };
            var properties = CreateExternalReferenceProperties();
            var externalReference = new ExternalReference();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectAddExternalReferenceReturns(usageId, properties, externalReference);

            //Act
            var result = _sut.AddExternalReference(usageUuid, properties);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(externalReference, result.Value);
        }

        [Fact]
        public void Add_ExternalReference_Returns_Error_When_Creation_Failed()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var usage = new ItSystemUsage { Id = usageId };
            var properties = CreateExternalReferenceProperties();
            var expectedOperationFailure = A<OperationFailure>();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectAddExternalReferenceReturns(usageId, properties, new OperationError(expectedOperationFailure));

            //Act
            var result = _sut.AddExternalReference(usageUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedOperationFailure, result.Error.FailureType);
        }

        [Fact]
        public void Add_ExternalReference_Returns_Forbidden_When_User_Has_No_Write_Access()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var usage = new ItSystemUsage { Id = usageId };
            var properties = CreateExternalReferenceProperties();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, false);

            //Act
            var result = _sut.AddExternalReference(usageUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Add_ExternalReference_Returns_Error_When_Get_Usage_Failed()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var properties = CreateExternalReferenceProperties();
            var expectedFailure = A<OperationFailure>();

            ExpectGetSystemUsageReturns(usageUuid, new OperationError(expectedFailure));

            //Act
            var result = _sut.AddExternalReference(usageUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_ExternalReference()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var referenceUuid = A<Guid>();
            var usage = new ItSystemUsage { Id = usageId };
            var properties = CreateExternalReferenceProperties();
            var externalReference = new ExternalReference();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectUpdateExternalReferenceReturns(usageId, referenceUuid, properties, externalReference);

            //Act
            var result = _sut.UpdateExternalReference(usageUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(externalReference, result.Value);
        }

        [Fact]
        public void Update_ExternalReference_Returns_Error_When_Update_Failed()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var referenceUuid = A<Guid>();
            var usage = new ItSystemUsage { Id = usageId };
            var properties = CreateExternalReferenceProperties();
            var expectedFailure = A<OperationFailure>();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectUpdateExternalReferenceReturns(usageId, referenceUuid, properties, new OperationError(expectedFailure));

            //Act
            var result = _sut.UpdateExternalReference(usageUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Update_ExternalReference_Returns_Forbidden_When_User_Has_No_Write_Access()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var referenceUuid = A<Guid>();
            var usage = new ItSystemUsage { Id = usageId };
            var properties = CreateExternalReferenceProperties();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, false);

            //Act
            var result = _sut.UpdateExternalReference(usageUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Update_ExternalReference_Returns_Error_When_GetUsage_Failed()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var referenceUuid = A<Guid>();
            var properties = CreateExternalReferenceProperties();
            var expectedFailure = A<OperationFailure>();

            ExpectGetSystemUsageReturns(usageUuid, new OperationError(expectedFailure));

            //Act
            var result = _sut.UpdateExternalReference(usageUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Can_Delete_ExternalReference()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var referenceUuid = A<Guid>();
            var referenceId = A<int>();
            var usage = new ItSystemUsage { Id = usageId };
            var externalReference = new ExternalReference();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ExternalReference>(referenceUuid, referenceId);
            ExpectRemoveExternalReferenceReturns(referenceId, externalReference);

            //Act
            var result = _sut.DeleteExternalReference(usageUuid, referenceUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(externalReference, result.Value);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_Error_When_Failed_To_Delete_Reference()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var referenceUuid = A<Guid>();
            var referenceId = A<int>();
            var usage = new ItSystemUsage { Id = usageId };
            var expectedFailure = A<OperationFailure>();

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ExternalReference>(referenceUuid, referenceId);
            ExpectRemoveExternalReferenceReturns(referenceId, expectedFailure);

            //Act
            var result = _sut.DeleteExternalReference(usageUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_NotFound_When_ExternalReferenceId_Was_Not_Found()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var referenceUuid = A<Guid>();
            var usage = new ItSystemUsage { Id = usageId };

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ExternalReference>(referenceUuid, Maybe<int>.None);

            //Act
            var result = _sut.DeleteExternalReference(usageUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_Forbidden_When_User_Has_No_Write_Access()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var referenceUuid = A<Guid>();
            var usage = new ItSystemUsage { Id = usageId };

            ExpectGetSystemUsageReturns(usageUuid, usage);
            ExpectAllowModifyReturns(usage, false);

            //Act
            var result = _sut.DeleteExternalReference(usageUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_Error_When_Failed_To_Get_Usage()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var referenceUuid = A<Guid>();
            var expectedFailure = A<OperationFailure>();

            ExpectGetSystemUsageReturns(usageUuid, new OperationError(expectedFailure));

            //Act
            var result = _sut.DeleteExternalReference(usageUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_Web_Accessibility_Properties()
        {
            var usage = new ItSystemUsage { Uuid = A<Guid>() };
            var org = new Organization { Uuid = A<Guid>() };
            SetupSimpleUpdate(usage, org);
            var parameters = new SystemUsageUpdateParameters { GeneralProperties = new UpdatedSystemUsageGeneralProperties { WebAccessibilityCompliance = A<YesNoPartiallyOption>().FromNullable().AsChangedValue(), LastWebAccessibilityCheck = A<DateTime>().FromNullable().AsChangedValue(), WebAccessibilityNotes = A<string>().AsChangedValue() } };

            var result = _sut.Update(usage.Uuid, parameters);

            Assert.True(result.Ok);
            var updatedSystem = result.Value;
            var generalProperties = parameters.GeneralProperties.Value;
            Assert.Equal(generalProperties.WebAccessibilityCompliance.NewValue, updatedSystem.WebAccessibilityCompliance);
            Assert.Equal(generalProperties.LastWebAccessibilityCheck.NewValue, updatedSystem.LastWebAccessibilityCheck);
            Assert.Equal(generalProperties.WebAccessibilityNotes.NewValue, updatedSystem.WebAccessibilityNotes);
        }

        [Fact]
        public void Can_Reset_Web_Accessibility_Properties()
        {
            var usage = new ItSystemUsage { Uuid = A<Guid>(), WebAccessibilityCompliance = A<YesNoPartiallyOption>(), LastWebAccessibilityCheck = A<DateTime>(), WebAccessibilityNotes = A<string>() };
            var org = new Organization { Uuid = A<Guid>() };
            SetupSimpleUpdate(usage, org);
            var parameters = new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    WebAccessibilityCompliance = Maybe<YesNoPartiallyOption>.None.AsChangedValue(),
                    LastWebAccessibilityCheck = Maybe<DateTime>.None.AsChangedValue(),
                    WebAccessibilityNotes =
                ((string)null).AsChangedValue()
                }
            };

            var result = _sut.Update(usage.Uuid, parameters);

            Assert.True(result.Ok);
            var updatedSystem = result.Value;
            Assert.Null(updatedSystem.WebAccessibilityCompliance);
            Assert.Null(updatedSystem.LastWebAccessibilityCheck);
            Assert.Null(updatedSystem.WebAccessibilityNotes);
        }

        private void SetupSimpleUpdate(ItSystemUsage usage, Organization org)
        {
            ExpectGetOrganizationReturns(org.Uuid, org);
            ExpectGetSystemUsageReturns(usage.Uuid, usage);
            ExpectAllowModifyReturns(usage, true);
            ExpectTransaction();
        }

        private void ExpectAddExternalReferenceReturns(int usageId, ExternalReferenceProperties properties, Result<ExternalReference, OperationError> result)
        {
            _referenceServiceMock.Setup(x => x.AddReference(usageId, ReferenceRootType.SystemUsage, properties)).Returns(result);
        }

        private void ExpectUpdateExternalReferenceReturns(int usageId, Guid externalReferenceUuid, ExternalReferenceProperties properties, Result<ExternalReference, OperationError> result)
        {
            _referenceServiceMock.Setup(x => x.UpdateReference(usageId, ReferenceRootType.SystemUsage, externalReferenceUuid, properties)).Returns(result);
        }

        private void ExpectRemoveExternalReferenceReturns(int externalReferenceId, Result<ExternalReference, OperationFailure> result)
        {
            _referenceServiceMock.Setup(x => x.DeleteByReferenceId(externalReferenceId)).Returns(result);
        }
        private void ExpectAddSystemRelationReturns(ItSystemUsage itSystemUsage, int toSystemUsageId, int? interfaceId, int? frequencyTypeId, int? contractId, string description, string urlReference, Result<SystemRelation, OperationError> result)
        {
            _systemUsageRelationServiceMock.Setup(x => x.AddRelation(itSystemUsage.Id, toSystemUsageId, interfaceId,
                description, urlReference, frequencyTypeId,
                contractId)).Returns(result);
        }

        private void ExpectModifyRelationReturns(ItSystemUsage itSystemUsage, int relationId, int toSystemUsageId, int? interfaceId, int? frequencyTypeId, int? contractId, string description, string urlReference, Result<SystemRelation, OperationError> result)
        {
            _systemUsageRelationServiceMock.Setup(x => x.ModifyRelation(itSystemUsage.Id, relationId, toSystemUsageId, description, urlReference, interfaceId, contractId, frequencyTypeId)).Returns(result);
        }


        private void ExpectRemoveRelationReturns(int systemUsageId, int relationDbId, Result<SystemRelation, OperationError> result)
        {
            _systemUsageRelationServiceMock.Setup(x => x.RemoveRelation(systemUsageId, relationDbId))
                .Returns(result);
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid? uuid, Maybe<int> dbId) where T : class, IHasUuid, IHasId
        {
            if (uuid.HasValue)
                _identityResolverMock.Setup(x => x.ResolveDbId<T>(uuid.Value)).Returns(dbId);
        }

        private static void AssertSystemUsageUpdateParametersWithSimpleParametersAdded(SystemUsageUpdateParameters expected, ItSystemUsage actual, bool shouldBeEmpty = false)
        {
            //General Properties
            var generalProperties = expected.GeneralProperties.Value;
            Assert.Equal(generalProperties.LocalCallName.NewValue, actual.LocalCallName);
            Assert.Equal(generalProperties.LocalSystemId.NewValue, actual.LocalSystemId);
            Assert.Equal(generalProperties.SystemVersion.NewValue, actual.Version);
            Assert.Equal(generalProperties.Notes.NewValue, actual.Note);

            if (shouldBeEmpty)
            {
                Assert.Null(actual.Concluded);
                Assert.Null(actual.ExpirationDate);
                Assert.Null(actual.ContainsAITechnology);
            }
            else
            {
                Assert.Equal(generalProperties.ValidFrom.NewValue.Value.Date, actual.Concluded);
                Assert.Equal(generalProperties.ValidTo.NewValue.Value.Date, actual.ExpirationDate);
                Assert.Equal(generalProperties.ContainsAITechnology.NewValue, actual.ContainsAITechnology);
            }

            //Archiving
            var archiving = expected.Archiving.Value;
            Assert.Equal(archiving.ArchiveDuty.NewValue, actual.ArchiveDuty);
            Assert.Equal(archiving.ArchiveActive.NewValue, actual.ArchiveFromSystem);
            Assert.Equal(archiving.ArchiveDocumentBearing.NewValue, actual.Registertype);
            Assert.Equal(archiving.ArchiveFrequencyInMonths.NewValue, actual.ArchiveFreq);
            Assert.Equal(archiving.ArchiveNotes.NewValue, actual.ArchiveNotes);

            //GDPR
            var gdpr = expected.GDPR.Value;
            Assert.Equal(gdpr.Purpose.NewValue, actual.GeneralPurpose);
            Assert.Equal(gdpr.BusinessCritical.NewValue, actual.isBusinessCritical);
            Assert.Equal(gdpr.HostedAt.NewValue, actual.HostedAt);
            Assert.Equal(gdpr.TechnicalPrecautionsInPlace.NewValue, actual.precautions);
            Assert.Equal(gdpr.UserSupervision.NewValue, actual.UserSupervision);
            Assert.Equal(gdpr.UserSupervisionDate.NewValue, actual.UserSupervisionDate);
            Assert.Equal(gdpr.RiskAssessmentConducted.NewValue, actual.riskAssessment);
            Assert.Equal(gdpr.RiskAssessmentConductedDate.NewValue, actual.riskAssesmentDate);
            Assert.Equal(gdpr.RiskAssessmentNotes.NewValue, actual.noteRisks);
            Assert.Equal(gdpr.PlannedRiskAssessmentDate.NewValue, actual.PlannedRiskAssessmentDate);
            Assert.Equal(gdpr.RiskAssessmentResult.NewValue, actual.preriskAssessment);
            Assert.Equal(gdpr.DPIAConducted.NewValue, actual.DPIA);
            Assert.Equal(gdpr.DPIADate.NewValue, actual.DPIADateFor);
            Assert.Equal(gdpr.RetentionPeriodDefined.NewValue, actual.answeringDataDPIA);
            Assert.Equal(gdpr.NextDataRetentionEvaluationDate.NewValue, actual.DPIAdeleteDate);

            if (shouldBeEmpty)
            {
                Assert.Equal(0, actual.numberDPIA);
                Assert.Null(actual.LinkToDirectoryUrlName);
                Assert.Null(actual.LinkToDirectoryUrl);
                Assert.Null(actual.TechnicalSupervisionDocumentationUrlName);
                Assert.Null(actual.TechnicalSupervisionDocumentationUrl);
                Assert.Null(actual.UserSupervisionDocumentationUrlName);
                Assert.Null(actual.UserSupervisionDocumentationUrl);
                Assert.Null(actual.RiskSupervisionDocumentationUrlName);
                Assert.Null(actual.RiskSupervisionDocumentationUrl);
                Assert.Null(actual.DPIASupervisionDocumentationUrlName);
                Assert.Null(actual.DPIASupervisionDocumentationUrl);
            }
            else
            {
                Assert.Equal(gdpr.DataRetentionEvaluationFrequencyInMonths.NewValue, actual.numberDPIA);
                AssertLink(gdpr.DirectoryDocumentation.NewValue.Value, actual.LinkToDirectoryUrlName, actual.LinkToDirectoryUrl);
                AssertLink(gdpr.TechnicalPrecautionsDocumentation.NewValue.Value, actual.TechnicalSupervisionDocumentationUrlName, actual.TechnicalSupervisionDocumentationUrl);
                AssertLink(gdpr.UserSupervisionDocumentation.NewValue.Value, actual.UserSupervisionDocumentationUrlName, actual.UserSupervisionDocumentationUrl);
                AssertLink(gdpr.RiskAssessmentDocumentation.NewValue.Value, actual.RiskSupervisionDocumentationUrlName, actual.RiskSupervisionDocumentationUrl);
                AssertLink(gdpr.DPIADocumentation.NewValue.Value, actual.DPIASupervisionDocumentationUrlName, actual.DPIASupervisionDocumentationUrl);
            }

        }

        private SystemUsageUpdateParameters CreateSystemUsageUpdateParametersWithSimpleParametersAdded()
        {
            DataOptions? userSupervision = DataOptions.YES;
            DataOptions? technicalPrecautionsInPlace = DataOptions.YES;
            DataOptions? riskAssessment = DataOptions.YES;
            DataOptions? dpiaConcluded = DataOptions.YES;
            DataOptions? retentionPeriodDefined = DataOptions.YES;
            return new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    LocalCallName = A<string>().AsChangedValue(),
                    LocalSystemId = A<string>().AsChangedValue(),
                    SystemVersion = A<string>().AsChangedValue(),
                    Notes = A<string>().AsChangedValue(),
                    LifeCycleStatus = A<LifeCycleStatusType?>().AsChangedValue(),
                    ValidFrom = Maybe<DateTime>.Some(DateTime.Now).AsChangedValue(),
                    ValidTo = Maybe<DateTime>.Some(DateTime.Now.AddDays(Math.Abs(A<short>()))).AsChangedValue(),
                    ContainsAITechnology = Maybe<YesNoUndecidedOption>.Some(A<YesNoUndecidedOption>()).AsChangedValue(),
                },
                Archiving = new UpdatedSystemUsageArchivingParameters
                {
                    ArchiveDuty = A<ArchiveDutyTypes?>().AsChangedValue(),
                    ArchiveActive = A<bool?>().AsChangedValue(),
                    ArchiveDocumentBearing = A<bool?>().AsChangedValue(),
                    ArchiveFrequencyInMonths = new ChangedValue<int?>(A<int>()),
                    ArchiveNotes = A<string>().AsChangedValue()
                },
                GDPR = new UpdatedSystemUsageGDPRProperties
                {
                    Purpose = A<string>().AsChangedValue(),
                    BusinessCritical = A<DataOptions?>().AsChangedValue(),
                    HostedAt = A<HostedAt?>().AsChangedValue(),
                    DirectoryDocumentation = A<NamedLink>().FromNullable().AsChangedValue(),
                    TechnicalPrecautionsInPlace = technicalPrecautionsInPlace.AsChangedValue(),
                    TechnicalPrecautionsDocumentation = A<NamedLink>().FromNullable().AsChangedValue(),
                    UserSupervision = userSupervision.AsChangedValue(),
                    UserSupervisionDate = A<DateTime?>().AsChangedValue(),
                    UserSupervisionDocumentation = A<NamedLink>().FromNullable().AsChangedValue(),
                    RiskAssessmentConducted = riskAssessment.AsChangedValue(),
                    RiskAssessmentConductedDate = A<DateTime?>().AsChangedValue(),
                    RiskAssessmentDocumentation = A<NamedLink>().FromNullable().AsChangedValue(),
                    RiskAssessmentNotes = A<string>().AsChangedValue(),
                    PlannedRiskAssessmentDate = A<DateTime?>().AsChangedValue(),
                    RiskAssessmentResult = A<RiskLevel?>().AsChangedValue(),
                    DPIAConducted = dpiaConcluded.AsChangedValue(),
                    DPIADate = A<DateTime?>().AsChangedValue(),
                    DPIADocumentation = A<NamedLink>().FromNullable().AsChangedValue(),
                    RetentionPeriodDefined = retentionPeriodDefined.AsChangedValue(),
                    NextDataRetentionEvaluationDate = A<DateTime?>().AsChangedValue(),
                    DataRetentionEvaluationFrequencyInMonths = A<int?>().AsChangedValue()
                }
            };
        }

        private static SystemUsageUpdateParameters CreateEmptySystemUsageUpdateParametersWithSimpleParametersAdded()
        {
            return new SystemUsageUpdateParameters
            {
                GeneralProperties = new UpdatedSystemUsageGeneralProperties
                {
                    LocalCallName = "".AsChangedValue(),
                    LocalSystemId = "".AsChangedValue(),
                    SystemVersion = "".AsChangedValue(),
                    Notes = "".AsChangedValue(),
                    LifeCycleStatus = new ChangedValue<LifeCycleStatusType?>(null),
                    ValidFrom = new ChangedValue<Maybe<DateTime>>(Maybe<DateTime>.None),
                    ValidTo = new ChangedValue<Maybe<DateTime>>(Maybe<DateTime>.None),
                    ContainsAITechnology = new ChangedValue<Maybe<YesNoUndecidedOption>>(Maybe<YesNoUndecidedOption>.None)
                },
                Archiving = new UpdatedSystemUsageArchivingParameters
                {
                    ArchiveDuty = new ChangedValue<ArchiveDutyTypes?>(null),
                    ArchiveActive = new ChangedValue<bool?>(null),
                    ArchiveDocumentBearing = new ChangedValue<bool?>(null),
                    ArchiveFrequencyInMonths = new ChangedValue<int?>(null),
                    ArchiveNotes = "".AsChangedValue()
                },
                GDPR = new UpdatedSystemUsageGDPRProperties
                {
                    Purpose = "".AsChangedValue(),
                    BusinessCritical = new ChangedValue<DataOptions?>(null),
                    HostedAt = new ChangedValue<HostedAt?>(null),
                    DirectoryDocumentation = new ChangedValue<Maybe<NamedLink>>(Maybe<NamedLink>.None),
                    TechnicalPrecautionsInPlace = new ChangedValue<DataOptions?>(null),
                    TechnicalPrecautionsDocumentation = new ChangedValue<Maybe<NamedLink>>(Maybe<NamedLink>.None),
                    UserSupervision = new ChangedValue<DataOptions?>(null),
                    UserSupervisionDate = new ChangedValue<DateTime?>(null),
                    UserSupervisionDocumentation = new ChangedValue<Maybe<NamedLink>>(Maybe<NamedLink>.None),
                    RiskAssessmentConducted = new ChangedValue<DataOptions?>(null),
                    RiskAssessmentConductedDate = new ChangedValue<DateTime?>(null),
                    RiskAssessmentDocumentation = new ChangedValue<Maybe<NamedLink>>(Maybe<NamedLink>.None),
                    RiskAssessmentNotes = new ChangedValue<string>(null),
                    PlannedRiskAssessmentDate = new ChangedValue<DateTime?>(null),
                    RiskAssessmentResult = new ChangedValue<RiskLevel?>(null),
                    DPIAConducted = new ChangedValue<DataOptions?>(null),
                    DPIADate = new ChangedValue<DateTime?>(null),
                    DPIADocumentation = new ChangedValue<Maybe<NamedLink>>(Maybe<NamedLink>.None),
                    RetentionPeriodDefined = new ChangedValue<DataOptions?>(null),
                    NextDataRetentionEvaluationDate = new ChangedValue<DateTime?>(null),
                    DataRetentionEvaluationFrequencyInMonths = new ChangedValue<int?>(null)
                }
            };
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
            Assert.Null(actual.ArchiveSupplier);
            Assert.Null(actual.ArchiveFromSystem);
            Assert.Null(actual.Registertype);
            Assert.Null(actual.ArchiveFreq);
            Assert.Equal("", actual.ArchiveNotes);
            _itSystemUsageServiceMock.Verify(x => x.RemoveAllArchivePeriods(actual.Id), Times.Once);
            _itSystemUsageServiceMock.Verify(x => x.AddArchivePeriod(actual.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        private void AssertArchivingParameters(UpdatedSystemUsageArchivingParameters expected, ItSystemUsage actual)
        {
            Assert.Equal(expected.ArchiveDuty.NewValue, actual.ArchiveDuty);
            Assert.Equal(expected.ArchiveTypeUuid.NewValue.Value, actual.ArchiveType.Uuid);
            Assert.Equal(expected.ArchiveLocationUuid.NewValue.Value, actual.ArchiveLocation.Uuid);
            Assert.Equal(expected.ArchiveTestLocationUuid.NewValue.Value, actual.ArchiveTestLocation.Uuid);
            Assert.Equal(expected.ArchiveSupplierOrganizationUuid.NewValue.Value, actual.ArchiveSupplier.Uuid);
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
                ArchiveJournalPeriods = Maybe<IEnumerable<SystemUsageJournalPeriodUpdate>>.None.AsChangedValue()
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
                ArchiveJournalPeriods = Many<SystemUsageJournalPeriodUpdate>().Select(p =>
                {
                    p.Uuid = null;
                    return p;
                }).ToList().FromNullable<IEnumerable<SystemUsageJournalPeriodUpdate>>().AsChangedValue()
            };
        }

        private void ExpectUpdateArchivePeriodReturns(int itSystemUsageId, Guid periodUuid, DateTime startDate, DateTime endDate, string archiveId, bool approved, Result<ArchivePeriod, OperationError> updateResult)
        {
            _itSystemUsageServiceMock.Setup(x => x.UpdateArchivePeriod(itSystemUsageId, periodUuid, startDate, endDate, archiveId, approved)).Returns(updateResult);
        }
        private void ExpectRemoveArchivePeriodReturns(int itSystemUsageId, Guid periodUuid, Result<ArchivePeriod, OperationError> removeResult)
        {
            _itSystemUsageServiceMock.Setup(x => x.RemoveArchivePeriod(itSystemUsageId, periodUuid)).Returns(removeResult);
        }
        private void ExpectAddArchivePeriodReturns(int itSystemUsageId, Result<ArchivePeriod, OperationError> addResult)
        {
            _itSystemUsageServiceMock.Setup(x => x.AddArchivePeriod(itSystemUsageId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(addResult);
        }

        private void ExpectAddArchivePeriodReturns(int itSystemUsageId, SystemUsageJournalPeriodProperties props, Result<ArchivePeriod, OperationError> addResult)
        {
            _itSystemUsageServiceMock.Setup(x => x.AddArchivePeriod(itSystemUsageId, props.StartDate, props.EndDate, props.ArchiveId, props.Approved)).Returns(addResult);
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

        private (Guid systemUuid, Guid organizationUuid, Mock<IDatabaseTransaction> transactionMock, Organization organization, ItSystem itSystem, ItSystemUsage itSystemUsage) CreateBasicTestVariables(bool assignUuidToOrganization = false)
        {
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var organization = CreateOrganization();
            if (assignUuidToOrganization)
                organization.Uuid = organizationUuid;

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

        private Organization CreateOrganization()
        {
            return new Organization { Id = A<int>() };
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
        private ExternalReferenceProperties CreateExternalReferenceProperties()
        {
            return new ExternalReferenceProperties(A<string>(), A<string>(), A<string>(), A<bool>());
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

        private void ExpectGetSystemUsageReturns(Guid systemUuid, Result<ItSystemUsage, OperationError> result)
        {
            _itSystemUsageServiceMock.Setup(x => x.GetReadableItSystemUsageByUuid(systemUuid)).Returns(result);
        }

        private void ExpectGetSystemReturns(Guid systemUuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(systemUuid)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid organizationUuid, Result<Organization, OperationError> organizationResult)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationUuid, null)).Returns(organizationResult);
        }

        private void ExpectGetReadableItSystemUsageByUuidReturns(Guid usageUuid,
            Result<ItSystemUsage, OperationError> result)
        {
            _itSystemUsageServiceMock.Setup(x => x.GetReadableItSystemUsageByUuid(usageUuid)).Returns(result);
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private static bool MatchExpectedAssignments(IEnumerable<(Guid roleUuid, Guid user)> actual, List<UserRolePair> expected)
        {
            return actual.SequenceEqual(expected.Select(p => (roleUuid: p.RoleUuid, user: p.UserUuid)));
        }

        private UpdatedSystemUsageArchivingParameters CreateUpdatedSystemUsageArchivingParameters()
        {
            var archivingParameters = new UpdatedSystemUsageArchivingParameters
            {
                ArchiveJournalPeriods = Many<SystemUsageJournalPeriodUpdate>(4).Select(p =>
                {
                    p.Uuid = null;
                    return p;
                }).ToList().FromNullable<IEnumerable<SystemUsageJournalPeriodUpdate>>().AsChangedValue()
            };
            return archivingParameters;
        }
    }
}
