using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Role;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Moq;
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
        private readonly Mock<IItContractService> _contractServiceMock;
        private readonly Mock<IItProjectService> _projectServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>> _roleAssignmentService;
        private readonly ItSystemUsageWriteService _sut;
        private readonly Mock<IKLEApplicationService> _kleServiceMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;

        public ItSystemUsageWriteServiceTest()
        {
            _itSystemUsageServiceMock = new Mock<IItSystemUsageService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _systemCategoriesOptionsServiceMock = new Mock<IOptionsService<ItSystemUsage, ItSystemCategories>>();
            _contractServiceMock = new Mock<IItContractService>();
            _projectServiceMock = new Mock<IItProjectService>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _kleServiceMock = new Mock<IKLEApplicationService>();
            _referenceServiceMock = new Mock<IReferenceService>();
            _roleAssignmentService = new Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>>();
            _sut = new ItSystemUsageWriteService(_itSystemUsageServiceMock.Object, _transactionManagerMock.Object,
                _itSystemServiceMock.Object, _organizationServiceMock.Object, _authorizationContextMock.Object,
                _systemCategoriesOptionsServiceMock.Object, _contractServiceMock.Object, _projectServiceMock.Object,
                _kleServiceMock.Object, _referenceServiceMock.Object, _roleAssignmentService.Object,
                Mock.Of<IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>>(),
                Mock.Of<IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage>>(),
                Mock.Of<IDatabaseControl>(), _domainEventsMock.Object, Mock.Of<ILogger>());
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
            Assert.True(createResult.Failed);
            Assert.Same(error, createResult.Error);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(error.FailureType, createResult.Error.FailureType);
            Assert.EndsWith(error.Message.Value, createResult.Error.Message.Value);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(error.FailureType, createResult.Error.FailureType);
            Assert.EndsWith(error.Message.Value, createResult.Error.Message.Value);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.Equal(generalProperties.LocalCallName.Value.Value, itSystemUsage.LocalCallName);
            Assert.Equal(generalProperties.LocalSystemId.Value.Value, itSystemUsage.LocalSystemId);
            Assert.Equal(generalProperties.SystemVersion.Value.Value, itSystemUsage.Version);
            Assert.Equal(generalProperties.Notes.Value.Value, itSystemUsage.Note);
            Assert.Equal(generalProperties.EnforceActive.Value.Value, itSystemUsage.Active);
            Assert.Equal(generalProperties.DataClassificationUuid.Value.Value, itSystemUsage.ItSystemCategories.Uuid);
            Assert.Equal(generalProperties.ValidFrom.Value.Value.Value.Date, itSystemUsage.Concluded);
            Assert.Equal(generalProperties.ValidTo.Value.Value.Value.Date, itSystemUsage.ExpirationDate);
            Assert.Equal(generalProperties.MainContractUuid.Value.Value, itSystemUsage.MainContract.ItContract.Uuid);
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
            AssertTransactionNotCommitted(transactionMock);
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
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
            AssertTransactionNotCommitted(transactionMock);
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
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
            Assert.True(createResult.Failed);
            Assert.Equal(operationError.FailureType, createResult.Error.FailureType);
            Assert.EndsWith(operationError.Message.Value, createResult.Error.Message.Value);
            AssertTransactionNotCommitted(transactionMock);

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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(createResult.Failed);
            AssertTransactionNotCommitted(transactionMock);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            Assert.Contains("Duplicates in KLE", createResult.Error.Message.GetValueOrFallback(string.Empty));
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
            Assert.True(createResult.Failed);
            AssertTransactionNotCommitted(transactionMock);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            Assert.EndsWith("KLE cannot be both added and removed", createResult.Error.Message.GetValueOrDefault());
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
            Assert.True(createResult.Failed);
            AssertTransactionNotCommitted(transactionMock);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            Assert.EndsWith("Cannot ADD KLE which is already present in the system context", createResult.Error.Message.GetValueOrDefault());
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
            Assert.True(createResult.Failed);
            AssertTransactionNotCommitted(transactionMock);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            Assert.EndsWith("Cannot Remove KLE which is not present in the system context", createResult.Error.Message.GetValueOrDefault());
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
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            AssertTransactionNotCommitted(transactionMock);
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
            Assert.True(result.Failed);
            AssertTransactionNotCommitted(transactionMock);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            Assert.EndsWith(operationError.Message.GetValueOrDefault(), result.Error.Message.GetValueOrDefault());
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
            Assert.True(createResult.Failed);
            AssertTransactionNotCommitted(transactionMock);
            Assert.Equal(error, createResult.Error.FailureType);
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
        private static SystemUsageUpdateParameters CreateSystemUsageUpdateParametersWithoutData()
        {
            return new SystemUsageUpdateParameters()
            {
                Roles = new UpdatedSystemUsageRoles()
                {
                    UserRolePairs = Maybe<IEnumerable<UserRolePair>>.None
                        .AsChangedValue()
                        .FromNullable()
                }
                    .FromNullable()
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
