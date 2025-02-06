using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;

using Moq;
using Serilog;
using Tests.Unit.Presentation.Web.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationServiceTest : OrganizationServiceTestBase
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly OrganizationService _sut;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly User _user;
        private readonly Mock<IGenericRepository<Organization>> _organizationRepository;
        private readonly Mock<IGenericRepository<OrganizationRight>> _orgRightRepository;
        private readonly Mock<IGenericRepository<User>> _userRepository;
        private readonly Mock<IOrganizationRepository> _repositoryMock;
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly Mock<IOrgUnitService> _orgUnitServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IOrganizationRightsService> _organizationRightsServiceMock;
        private readonly Mock<IGenericRepository<ContactPerson>> _contactPersonRepository;
        private readonly Mock<IEntityIdentityResolver> _identityResolver;
        private readonly Mock<IGenericRepository<DataResponsible>> _dataResponsibleRepository;
        private readonly Mock<IGenericRepository<DataProtectionAdvisor>> _dataProtectionAdvisorRepository;

        public OrganizationServiceTest()
        {
            _user = new User { Id = new Fixture().Create<int>() };
            _authorizationContext = new Mock<IAuthorizationContext>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _userContext.Setup(x => x.UserId).Returns(_user.Id);
            _userContext.Setup(x => x.OrganizationIds).Returns(new Fixture().Create<IEnumerable<int>>());
            _organizationRepository = new Mock<IGenericRepository<Organization>>();
            _orgRightRepository = new Mock<IGenericRepository<OrganizationRight>>();
            _userRepository = new Mock<IGenericRepository<User>>();
            _repositoryMock = new Mock<IOrganizationRepository>();
            _orgUnitServiceMock = new Mock<IOrgUnitService>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            _contactPersonRepository = new Mock<IGenericRepository<ContactPerson>>();
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _dataResponsibleRepository = new Mock<IGenericRepository<DataResponsible>>();
            _dataProtectionAdvisorRepository = new Mock<IGenericRepository<DataProtectionAdvisor>>();

            _sut = new OrganizationService(
                _organizationRepository.Object,
                _orgRightRepository.Object,
                _contactPersonRepository.Object,
                _userRepository.Object,
                _authorizationContext.Object,
                _userContext.Object,
                Mock.Of<ILogger>(),
                _transactionManager.Object,
                _repositoryMock.Object,
                _organizationRightsServiceMock.Object,
                _orgUnitServiceMock.Object,
                _domainEventsMock.Object,
                _identityResolver.Object,
                _dataResponsibleRepository.Object,
                _dataProtectionAdvisorRepository.Object);
        }

        [Fact]
        public void Can_Get_UI_Root_Config()
        {
            var orgUuid = A<Guid>();
            var org = new Organization()
            {
                Uuid = orgUuid,
                Config = new Config()
                {
                    Id = A<int>(),
                    ShowItSystemModule = A<bool>(),
                    ShowDataProcessing = A<bool>()
                }
            };
            _repositoryMock.Setup(_ => _.GetByUuid(orgUuid)).Returns(org);
            _authorizationContext.Setup(_ => _.AllowReads(org)).Returns(true);

            var uiRootConfig = _sut.GetUIRootConfig(orgUuid);

            Assert.True(uiRootConfig.Ok);
            var expected = org.Config;
            var value = uiRootConfig.Value;
            Assert.Equal(expected.Id, value.Id);
            Assert.Equal(expected.ShowItSystemModule, value.ShowItSystemModule);
            Assert.Equal(expected.ShowDataProcessing, value.ShowDataProcessing);
        }

        [Fact]
        public void Get_UI_Root_Config_Returns_Not_Found_If_No_Org()
        {
            var orgUuid = A<Guid>();
            var org = new Organization()
            {
                Uuid = orgUuid,
                Config = new Config()
                {
                    Id = A<int>(),
                    ShowItSystemModule = A<bool>(),
                    ShowDataProcessing = A<bool>()
                }
            };
            _repositoryMock.Setup(_ => _.GetByUuid(orgUuid)).Returns(Maybe<Organization>.None);
            _authorizationContext.Setup(_ => _.AllowReads(org)).Returns(true);

            var uiRootConfig = _sut.GetUIRootConfig(orgUuid);

            Assert.True(uiRootConfig.Failed);
            Assert.Equal(OperationFailure.NotFound, uiRootConfig.Error.FailureType);
        }

        [Fact]
        public void Get_UI_Root_Config_Returns_Forbidden_If_No_Read_Access()
        {
            var orgUuid = A<Guid>();
            var org = new Organization()
            {
                Uuid = orgUuid,
                Config = new Config()
                {
                    Id = A<int>(),
                    ShowItSystemModule = A<bool>(),
                    ShowDataProcessing = A<bool>()
                }
            };
            _repositoryMock.Setup(_ => _.GetByUuid(orgUuid)).Returns(org);
            _authorizationContext.Setup(_ => _.AllowReads(org)).Returns(false);

            var uiRootConfig = _sut.GetUIRootConfig(orgUuid);

            Assert.True(uiRootConfig.Failed);
            Assert.Equal(OperationFailure.Forbidden, uiRootConfig.Error.FailureType);
        }

        [Fact]
        public void CanCreateOrganizationOfType_With_Null_Org_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.CanChangeOrganizationType(null, A<OrganizationTypeKeys>()));
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void CanChangeOrganizationType_Returns(bool allowModify, bool allowChangeOrgType, bool expectedResult)
        {
            //Arrange
            var organization = new Organization();
            var organizationTypeKeys = A<OrganizationTypeKeys>();
            _authorizationContext.Setup(x => x.AllowModify(organization)).Returns(allowModify);
            _authorizationContext.Setup(x => x.HasPermission(It.IsAny<DefineOrganizationTypePermission>())).Returns(allowChangeOrgType);

            //Act
            var result = _sut.CanChangeOrganizationType(organization, organizationTypeKeys);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CreateNewOrganization_Throws_On_Null_Arg()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.CreateNewOrganization(default(Organization)));
        }

        [Fact]
        public void CreateNewOrganization_Returns_BadInput_On_Invalid_Cvr()
        {
            //Arrange
            var newOrg = new Organization { Cvr = "monkey" };
            _userRepository.Setup(x => x.GetByKey(_user.Id)).Returns(_user);

            //Act
            var result = _sut.CreateNewOrganization(newOrg);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void CreateNewOrganization_Returns_Forbidden_On_Lost_User()
        {
            //Arrange
            var newOrg = new Organization { Cvr = "monkey" };
            _userRepository.Setup(x => x.GetByKey(_user.Id)).Returns(default(User));

            //Act
            var result = _sut.CreateNewOrganization(newOrg);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Theory]
        [InlineData("23123123")]
        public void CreateNewOrganization_Returns_Forbidden(string cvr)
        {
            //Arrange
            var newOrg = new Organization { Cvr = cvr };
            ExpectAllowCreateReturns<Organization>(false);

            //Act
            var result = _sut.CreateNewOrganization(newOrg);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Theory]
        [InlineData(OrganizationTypeKeys.Kommune)]
        [InlineData(OrganizationTypeKeys.Virksomhed)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed)]
        public void CreateNewOrganization_Returns_Ok(OrganizationTypeKeys organizationType)
        {
            //Arrange
            var newOrg = new Organization
            {
                Name = A<string>(),
                TypeId = (int)organizationType
            };
            _userRepository.Setup(x => x.GetByKey(_user.Id)).Returns(_user);
            ExpectAllowCreateReturns<Organization>(true);
            _authorizationContext.Setup(x => x.HasPermission(It.IsAny<DefineOrganizationTypePermission>()))
                .Returns(true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            _organizationRepository.Setup(x => x.Insert(newOrg)).Returns(newOrg);

            //Act
            var result = _sut.CreateNewOrganization(newOrg);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(newOrg, result.Value);
            transaction.Verify(x => x.Commit(), Times.Once);
            Assert.Equal(1, newOrg.OrgUnits.Count);
            Assert.Equal(newOrg.Name, newOrg.OrgUnits.First().Name);
            Assert.NotNull(newOrg.Config);
            _organizationRepository.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateNewOrganization_Assigns_Uuid_If_Not_Defined(bool useEmptyGuid)
        {
            //Arrange
            var originalUuid = useEmptyGuid ? Guid.Empty : Guid.NewGuid();
            var newOrg = new Organization
            {
                Name = A<string>(),
                TypeId = (int)OrganizationTypeKeys.Kommune,
                Uuid = originalUuid
            };
            _userRepository.Setup(x => x.GetByKey(_user.Id)).Returns(_user);
            ExpectAllowCreateReturns<Organization>(true);
            _authorizationContext.Setup(x => x.HasPermission(It.IsAny<DefineOrganizationTypePermission>())).Returns(true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            _organizationRepository.Setup(x => x.Insert(newOrg)).Returns(newOrg);

            //Act
            var result = _sut.CreateNewOrganization(newOrg);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(newOrg, result.Value);
            if (useEmptyGuid)
            {
                Assert.NotEqual(originalUuid, result.Value.Uuid);
            }
            else
            {
                Assert.Equal(originalUuid, result.Value.Uuid);
            }
        }

        [Fact]
        public void RemoveUser_Returns_NotFound()
        {
            //Arrange
            var organizationId = A<int>();
            var userId = A<int>();
            ExpectGetOrganizationByKeyReturns(organizationId, null);

            //Act
            var result = _sut.RemoveUser(organizationId, userId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void RemoveUser_Returns_Forbidden()
        {
            //Arrange
            var organizationId = A<int>();
            var userId = A<int>();
            var organization = new Organization();
            ExpectGetOrganizationByKeyReturns(organizationId, organization);
            ExpectAllowModifyReturns(organization, false);

            //Act
            var result = _sut.RemoveUser(organizationId, userId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void RemoveUser_Returns_Ok()
        {
            //Arrange
            var organizationId = A<int>();
            var userId = A<int>();
            var organization = new Organization();
            ExpectGetOrganizationByKeyReturns(organizationId, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectTransactionBeginReturns();
            var matchedRight1 = CreateRight(organizationId, userId);
            var matchedRight2 = CreateRight(organizationId, userId);
            var unmatchedRight1 = CreateRight(A<int>(), userId);
            var unmatchedRight2 = CreateRight(organizationId, A<int>());
            var rightsArray = new[] {matchedRight1, unmatchedRight1, matchedRight2, unmatchedRight2};
            _orgRightRepository.Setup(x => x.AsQueryable()).Returns(rightsArray.AsQueryable());
            ExpectOrganizationRightsRemoveRoleReturnsSuccess(rightsArray);

            //Act
            var result = _sut.RemoveUser(organizationId, userId);

            //Assert that only the right entities were removed
            Assert.True(result.Ok);
            _organizationRightsServiceMock.Verify(x => x.RemoveRole(matchedRight1.Id), Times.Once);
            _organizationRightsServiceMock.Verify(x => x.RemoveRole(matchedRight2.Id), Times.Once);
            _organizationRightsServiceMock.Verify(x => x.RemoveRole(unmatchedRight1.Id), Times.Never);
            _organizationRightsServiceMock.Verify(x => x.RemoveRole(unmatchedRight2.Id), Times.Never);
        }

        [Fact]
        public void GetAllOrganizations_Returns_All()
        {
            //Arrange
            var expectedOrg1 = new Organization() { Id = A<int>() };
            var expectedOrg2 = new Organization() { Id = A<int>() };
            var expectedOrg3 = new Organization() { Id = A<int>() };
            _repositoryMock.Setup(x => x.GetAll()).Returns(new List<Organization> { expectedOrg1, expectedOrg2, expectedOrg3 }.AsQueryable());
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.GetAllOrganizations();

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(3, result.Value.Count());
            Assert.Same(expectedOrg1, result.Value.First(x => x.Id == expectedOrg1.Id));
            Assert.Same(expectedOrg2, result.Value.First(x => x.Id == expectedOrg2.Id));
            Assert.Same(expectedOrg3, result.Value.First(x => x.Id == expectedOrg3.Id));
        }

        [Theory]
        [InlineData(CrossOrganizationDataReadAccessLevel.None)]
        [InlineData(CrossOrganizationDataReadAccessLevel.Public)]
        [InlineData(CrossOrganizationDataReadAccessLevel.RightsHolder)]
        public void GetOrganizations_Returns_Forbidden_If_Not_CrossOrganizationDataReadAccessLevel_All(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            _repositoryMock.Setup(x => x.GetAll()).Returns(new List<Organization>() { new() }.AsQueryable());
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);

            //Act
            var result = _sut.GetAllOrganizations();

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void SearchAccessibleOrganizations_DoesNotFilterByOrgMembership_If_Cross_Access_Is_All()
        {
            //Arrange
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.All);
            var allValues = new[] { new Organization(), new Organization() };
            _repositoryMock.Setup(x => x.GetAll()).Returns(allValues.AsQueryable());

            //Act
            var organizations = _sut.SearchAccessibleOrganizations().ToList();

            //Assert
            Assert.Equal(allValues, organizations);
        }

        [Fact]
        public void SearchAccessibleOrganizations_Is_Filtered_By_Access_And_Sharing()
        {
            //Arrange
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.Public);
            var org1WithMembership = A<int>();
            var org2WithMembership = A<int>();
            var org1WithoutMembership = A<int>();
            var org2WithoutMembership = A<int>();

            var expected1 = new Organization { Id = org1WithMembership, AccessModifier = AccessModifier.Local };
            var expected2 = new Organization { Id = org2WithMembership, AccessModifier = AccessModifier.Public };
            var expected3 = new Organization { Id = org1WithoutMembership, AccessModifier = AccessModifier.Public };
            var excluded = new Organization { Id = org2WithoutMembership, AccessModifier = AccessModifier.Local };

            var allValues = new[] { expected1, expected2, excluded, expected3 };

            _repositoryMock.Setup(x => x.GetAll()).Returns(allValues.AsQueryable());
            _userContext.Setup(x => x.OrganizationIds).Returns(new[] { org1WithMembership, org2WithMembership });

            //Act
            var organizations = _sut.SearchAccessibleOrganizations().ToList();

            //Assert
            Assert.Equal(allValues.Except(excluded.WrapAsEnumerable()).ToList(), organizations.ToList());
        }

        [Fact]
        public void SearchAccessibleOrganizations_Is_Filtered_By_RequestedMembership()
        {
            //Arrange
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.Public);
            var org1WithMembership = A<int>();
            var org2WithMembership = A<int>();
            var org1WithoutMembership = A<int>();
            var org2WithoutMembership = A<int>();

            var expected1 = new Organization { Id = org1WithMembership, AccessModifier = AccessModifier.Local };
            var expected2 = new Organization { Id = org2WithMembership, AccessModifier = AccessModifier.Public };
            var excluded1 = new Organization { Id = org1WithoutMembership, AccessModifier = AccessModifier.Public };
            var excluded2 = new Organization { Id = org2WithoutMembership, AccessModifier = AccessModifier.Local };

            var allValues = new[] { expected1, expected2, excluded1, excluded2 };

            _repositoryMock.Setup(x => x.GetAll()).Returns(allValues.AsQueryable());
            _userContext.Setup(x => x.OrganizationIds).Returns(new[] { org1WithMembership, org2WithMembership });

            //Act
            var organizations = _sut.SearchAccessibleOrganizations(true).ToList();

            //Assert
            Assert.Equal(allValues.Except(new []{excluded1,excluded2}).ToList(), organizations.ToList());
        }

        [Theory]
        [InlineData(CrossOrganizationDataReadAccessLevel.RightsHolder)]
        [InlineData(CrossOrganizationDataReadAccessLevel.None)]
        public void SearchAccessibleOrganizations_Is_Filtered_By_MembershipOnly(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);
            var org1WithMembership = A<int>();
            var org2WithMembership = A<int>();
            var org1WithoutMembership = A<int>();

            var expected1 = new Organization { Id = org1WithMembership, AccessModifier = AccessModifier.Local };
            var expected2 = new Organization { Id = org2WithMembership, AccessModifier = AccessModifier.Public };
            var excluded = new Organization { Id = org1WithoutMembership, AccessModifier = AccessModifier.Public };

            var allValues = new[] { expected1, expected2, excluded };

            _repositoryMock.Setup(x => x.GetAll()).Returns(allValues.AsQueryable());
            _userContext.Setup(x => x.OrganizationIds).Returns(new[] { org1WithMembership, org2WithMembership });

            //Act
            var organizations = _sut.SearchAccessibleOrganizations().ToList();

            //Assert
            Assert.Equal(allValues.Except(excluded.WrapAsEnumerable()).ToList(), organizations.ToList());
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.All)]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void GetOrganization_Returns_Success(OrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            var uuid = A<Guid>();
            var expectedOrg = new Organization { Id = A<int>() };
            ExpectGetOrganizationByUuidReturns(uuid, expectedOrg);
            ExpectAllowReadOrganizationReturns(expectedOrg, true);
            ExpectGetOrganizationReadAccessLevelReturns(expectedOrg.Id, accessLevel);

            //Act
            var result = _sut.GetOrganization(uuid, accessLevel);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedOrg, result.Value);
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.Public, OrganizationDataReadAccessLevel.All)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder, OrganizationDataReadAccessLevel.All)]
        [InlineData(OrganizationDataReadAccessLevel.None, OrganizationDataReadAccessLevel.All)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder, OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.None, OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.None, OrganizationDataReadAccessLevel.RightsHolder)]
        public void GetOrganization_Returns_Forbidden_If_OrganizationDataReadAccessLevel_Is_Lower_Than_Required(OrganizationDataReadAccessLevel expectedAccessLevel, OrganizationDataReadAccessLevel requiredAccessLevel)
        {
            //Arrange
            var uuid = A<Guid>();
            var expectedOrg = new Organization();
            ExpectGetOrganizationByUuidReturns(uuid, expectedOrg);
            ExpectGetOrganizationReadAccessLevelReturns(expectedOrg.Id, expectedAccessLevel);

            //Act
            var result = _sut.GetOrganization(uuid, requiredAccessLevel);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetOrganization_Returns_Forbidden_If_No_ReadAccess_On_Organization()
        {
            //Arrange
            var uuid = A<Guid>();
            var expectedOrg = new Organization();
            ExpectGetOrganizationByUuidReturns(uuid, expectedOrg);
            ExpectGetOrganizationReadAccessLevelReturns(expectedOrg.Id, OrganizationDataReadAccessLevel.All);
            ExpectAllowReadOrganizationReturns(expectedOrg, false);

            //Act
            var result = _sut.GetOrganization(uuid, null);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetOrganization_Returns_NotFound()
        {
            //Arrange
            var uuid = A<Guid>();
            ExpectGetOrganizationByUuidReturns(uuid, Maybe<Organization>.None);

            //Act
            var result = _sut.GetOrganization(uuid, OrganizationDataReadAccessLevel.All);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetOrganizationUnits_Returns_All_Units_Filtered()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization() { Id = A<int>() };
            var allOrgUnits = new[] { new OrganizationUnit(), new OrganizationUnit(), new OrganizationUnit() }.AsQueryable();
            var filteredUnits = allOrgUnits.Skip(1);
            var orgUnitQueryMock = new Mock<IDomainQuery<OrganizationUnit>>();

            orgUnitQueryMock.Setup(x => x.Apply(allOrgUnits)).Returns(filteredUnits);
            ExpectGetOrganizationByUuidReturns(organizationId, organization);
            ExpectGetOrganizationAccessLevel(organization.Id, OrganizationDataReadAccessLevel.All);
            _orgUnitServiceMock.Setup(x => x.GetOrganizationUnits(organization)).Returns(allOrgUnits);

            //Act
            var result = _sut.GetOrganizationUnits(organizationId, orgUnitQueryMock.Object);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(filteredUnits.ToList(), result.Value.ToList());
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void Cannot_Get_Organization_Units_Unless_Full_Access_To_Organization(OrganizationDataReadAccessLevel organizationDataReadAccessLevel)
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization() { Id = A<int>() };
            ExpectGetOrganizationByUuidReturns(organizationId, organization);
            ExpectGetOrganizationAccessLevel(organization.Id, organizationDataReadAccessLevel);

            //Act
            var result = _sut.GetOrganizationUnits(organizationId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Get_Organization_Units_If_Organization_Is_Not_Found()
        {
            //Arrange
            var organizationId = A<Guid>();
            ExpectGetOrganizationByUuidReturns(organizationId, Maybe<Organization>.None);

            //Act
            var result = _sut.GetOrganizationUnits(organizationId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void ComputeOrganizationRemovalConflicts_Returns_NoConflicts()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);

            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Ok);
            Assert.False(result.Value.Any);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public void ComputeOrganizationRemovalConflicts_Returns_Forbidden_If_No_Access(bool allowRead, bool allowDelete)
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(allowRead, allowDelete);

            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void ComputeOrganizationRemovalConflicts_Returns_SystemsWithUsagesInOtherOrganizations()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var anotherOrg = CreateOrganization();
            organization.ItSystems.Add(new ItSystem { Usages = { CreateItSystemUsage(organization) } }.InOrganization(organization)); //not included - it belongs to deleted organization
            var match1 = new ItSystem { Usages = { CreateItSystemUsage(organization), CreateItSystemUsage(anotherOrg) } }.InOrganization(organization); //used both inside and outside the org
            var match2 = new ItSystem { Usages = { CreateItSystemUsage(anotherOrg) } }.InOrganization(organization); //only used outside the org
            organization.ItSystems.Add(match1);
            organization.ItSystems.Add(match2);

            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Ok);
            var conflicts = result.Value;
            Assert.True(conflicts.Any);
            Assert.Equal(new[] { match1, match2 }, conflicts.SystemsWithUsagesOutsideTheOrganization);
        }

        [Fact]
        public void ComputeOrganizationRemovalConflicts_Returns_InterfacesExposedOutsideTheOrganization()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var anotherOrg = CreateOrganization();
            var systemInDeletedOrg = new ItSystem().InOrganization(organization);
            var systemInAnotherOrg = new ItSystem().InOrganization(anotherOrg);
            organization.ItInterfaces.Add(new ItInterface().ExhibitedBy(systemInDeletedOrg).InOrganization(organization)); //not included it is exposed on the deleted org so fine to nuke
            organization.ItInterfaces.Add(new ItInterface()); //not included because no exhibits so fine to nuke it
            var match1 = new ItInterface().ExhibitedBy(systemInAnotherOrg).InOrganization(organization); //exhibited by system owned by another organization
            organization.ItInterfaces.Add(match1);

            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Ok);
            var conflicts = result.Value;
            Assert.True(conflicts.Any);
            Assert.Equal(new[] { match1 }, conflicts.InterfacesExposedOnSystemsOutsideTheOrganization);
        }

        [Fact]
        public void ComputeOrganizationRemovalConflicts_Returns_SystemsExposingInterfacesDefinedInOtherOrganizations()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var anotherOrg = CreateOrganization();
            var interface1InOwnOrg = CreateInterface().InOrganization(organization);
            var interface2InOwnOrg = CreateInterface().InOrganization(organization);
            var interface1InAnotherOrg = CreateInterface().InOrganization(anotherOrg);
            var interface2InAnotherOrg = CreateInterface().InOrganization(anotherOrg);
            organization.ItSystems.Add(new ItSystem().WithInterfaceExhibit(interface1InOwnOrg).InOrganization(organization)); //not included - it belongs to deleted organization
            var match1 = new ItSystem().WithInterfaceExhibit(interface2InOwnOrg).WithInterfaceExhibit(interface1InAnotherOrg).InOrganization(organization); //exhibited on interfaces both inside and outside the org
            var match2 = new ItSystem().WithInterfaceExhibit(interface2InAnotherOrg).InOrganization(organization); //only exhibited by interface outside the org
            organization.ItSystems.Add(match1);
            organization.ItSystems.Add(match2);

            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Ok);
            var conflicts = result.Value;
            Assert.True(conflicts.Any);
            Assert.Equal(new[] { match1, match2 }, conflicts.SystemsExposingInterfacesDefinedInOtherOrganizations);
        }

        [Fact]
        public void ComputeOrganizationRemovalConflicts_Returns_SystemsSetAsParentSystemToSystemsInOtherOrganizations()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var anotherOrg = CreateOrganization();
            var systemInSameOrg = CreateItSystem().InOrganization(organization);

            organization.ItSystems.Add(CreateItSystem().InOrganization(organization)); //not included - no parent
            organization.ItSystems.Add(CreateItSystem().InOrganization(organization).WithParentSystem(systemInSameOrg)); //not included - parent belongs to deleted organization
            var match = CreateItSystem().InOrganization(organization);
            CreateItSystem().InOrganization(anotherOrg).WithParentSystem(match); //Set a "match" as parent to a system in another org
            organization.ItSystems.Add(match);

            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Ok);
            var conflicts = result.Value;
            Assert.True(conflicts.Any);
            Assert.Equal(new[] { match }, conflicts.SystemsSetAsParentSystemToSystemsInOtherOrganizations);
        }

        [Fact]
        public void ComputeOrganizationRemovalConflicts_Returns_SystemsInOtherOrganizationsWhereOrgIsRightsHolder()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var anotherOrg = CreateOrganization();

            organization.ItContracts.Add(CreateContract().InOrganization(organization)); //not included - no supplier
            organization.ItContracts.Add(CreateContract().InOrganization(organization).WithSupplier(organization)); //not included - supplier is deleted org
            var match = CreateContract().InOrganization(anotherOrg).WithSupplier(organization);
            organization.ItContracts.Add(match);

            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Ok);
            var conflicts = result.Value;
            Assert.True(conflicts.Any);
            Assert.Equal(new[] { match }, conflicts.ContractsInOtherOrganizationsWhereOrgIsSupplier);
        }

        [Fact]
        public void ComputeOrganizationRemovalConflicts_Returns_DprConflicts()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var anotherOrg = CreateOrganization();

            var match1 = CreateDpr().InOrganization(anotherOrg).WithDataProcessor(organization);
            var match2 = CreateDpr().InOrganization(anotherOrg).WithSubDataProcessor(organization);
            var match3 = CreateDpr().InOrganization(anotherOrg).WithSubDataProcessor(organization).WithDataProcessor(organization);

            CreateDpr().InOrganization(organization).WithDataProcessor(organization).WithDataProcessor(anotherOrg).WithSubDataProcessor(anotherOrg); //not included since DPR is in same org as deleted org


            //Act
            var result = _sut.ComputeOrganizationRemovalConflicts(organization.Uuid);

            //Assert
            Assert.True(result.Ok);
            var conflicts = result.Value;
            Assert.True(conflicts.Any);
            Assert.Equal(new[] { match1, match3 }, conflicts.DprInOtherOrganizationsWhereOrgIsDataProcessor);
            Assert.Equal(new[] { match2, match3 }, conflicts.DprInOtherOrganizationsWhereOrgIsSubDataProcessor);
        }

        [Fact]
        public void Delete_Returns_Ok_Of_No_Conflicts()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.RemoveOrganization(organization.Uuid, false);

            //Assert
            VerifyOrganizationDeleted(result, transaction, organization);
        }

        [Fact]
        public void Delete_Returns_Conflict_If_Conflicts_And_EnforceDeletion_Is_False()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            organization.ItSystems.Add(CreateItSystem().InOrganization(organization).WithInterfaceExhibit(CreateInterface().InOrganization(CreateOrganization()))); //Create a conflict
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.RemoveOrganization(organization.Uuid, false);

            //Assert
            _organizationRepository.Verify(x => x.Save(), Times.Never());
            transaction.Verify(x => x.Commit(), Times.Never());
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityBeingDeletedEvent<Organization>>(org => org.Entity == organization)), Times.Never());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Conflict, result.Value.FailureType);
        }

        [Fact]
        public void Delete_Returns_Ok_If_Conflicts_And_EnforceDeletion_Is_True()
        {
            //Arrange
            var organization = SetupConflictCalculationPrerequisites(true, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.RemoveOrganization(organization.Uuid, true);

            //Assert
            VerifyOrganizationDeleted(result, transaction, organization);
        }

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        [InlineData(true, true, false, false)]
        [InlineData(true, false, true, true)]
        [InlineData(true, false, true, false)]
        [InlineData(true, false, false, true)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, true, true)]
        [InlineData(false, true, true, false)]
        [InlineData(false, true, false, true)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        [InlineData(false, false, false, false)]

        public void Can_Get_Permissions(bool read, bool modify, bool delete, bool modifyCvr)
        {
            //Arrange
            var uuid = A<Guid>();
            var organization = new Organization { Uuid = uuid };
            ExpectGetOrganizationByUuidReturns(uuid, organization);
            _authorizationContext.SetupSequence(_ => _.AllowReads(organization))
                .Returns(true)
                .Returns(read);
            ExpectAllowModifyReturns(organization, modify);
            _userContext.Setup(_ => _.IsGlobalAdmin()).Returns(modifyCvr);
            _authorizationContext.Setup(x => x.AllowDelete(organization)).Returns(delete);
            _repositoryMock.Setup(_ => _.GetByUuid(uuid)).Returns(organization);
            _authorizationContext.Setup(_ => _.GetOrganizationReadAccessLevel(organization.Id))
                .Returns(OrganizationDataReadAccessLevel.All);
            //Act
            var result = _sut.GetPermissions(uuid);

            //Assert
            Assert.True(result.Ok);
            var permissions = result.Value;
            Assert.Equivalent(new OrganizationPermissionsResult(new ResourcePermissionsResult(read, modify, delete), modifyCvr), permissions);
        }

        [Fact]
        public void Can_Get_Permissions_Returns_Not_Found()
        {
            //Arrange
            var wrongUuid = A<Guid>();
            ExpectGetOrganizationByUuidReturns(wrongUuid, Maybe<Organization>.None);

            //Act
            var result = _sut.GetPermissions(wrongUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, false)]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        public void Get_Grid_Permissions_Returns_Expected_Result(OrganizationRole roleBeingAsked, bool shouldHaveModificationPermission)
        {
            var org = CreateOrganization();

            _userContext.Setup(x => x.HasRole(org.Id, roleBeingAsked)).Returns(true);
            var permissions = _sut.GetGridPermissions(org.Id);
            Assert.Equal(shouldHaveModificationPermission, permissions.ConfigModificationPermission);
        }

        private void VerifyOrganizationDeleted(Maybe<OperationError> result, Mock<IDatabaseTransaction> transaction, Organization organization)
        {
            Assert.True(result.IsNone);
            _organizationRepository.Verify(x => x.Save(), Times.Once());
            _organizationRepository.Verify(x => x.DeleteWithReferencePreload(organization), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
            _domainEventsMock.Verify(
                x => x.Raise(It.Is<EntityBeingDeletedEvent<Organization>>(org => org.Entity == organization)), Times.Once());
        }

        private ItInterface CreateInterface()
        {
            return new ItInterface { Id = A<int>(), Uuid = A<Guid>() };
        }

        private ItContract CreateContract()
        {
            return new ItContract { Id = A<int>(), Uuid = A<Guid>() };
        }

        private ItSystem CreateItSystem()
        {
            return new ItSystem { Id = A<int>(), Uuid = A<Guid>() };
        }


        private static ItSystemUsage CreateItSystemUsage(Organization organization)
        {
            return new ItSystemUsage { OrganizationId = organization.Id, Organization = organization };
        }

        private static DataProcessingRegistration CreateDpr()
        {
            return new DataProcessingRegistration();
        }

        private Organization SetupConflictCalculationPrerequisites(bool allowRead, bool allowDelete)
        {
            var organization = CreateOrganization();
            ExpectGetOrganizationByUuidReturns(organization.Uuid, organization);
            ExpectAllowReadOrganizationReturns(organization, allowRead);
            ExpectAllowDeleteReturns(organization, allowDelete);
            return organization;
        }

        private void ExpectTransactionBeginReturns()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
        }
        

        private void ExpectOrganizationRightsRemoveRoleReturnsSuccess(OrganizationRight[] rights)
        {
            foreach (var right in rights)
            {
                _organizationRightsServiceMock.Setup(x => x.RemoveRole(right.Id)).Returns(right);
            }
        }

        private void ExpectAllowDeleteReturns(Organization organization, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(organization)).Returns(value);
        }

        private void ExpectGetOrganizationAccessLevel(int organizationId, OrganizationDataReadAccessLevel organizationDataReadAccessLevel)
        {
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(organizationDataReadAccessLevel);
        }

        private void ExpectAllowReadOrganizationReturns(Organization expectedOrg, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(expectedOrg)).Returns(value);
        }

        private void ExpectGetOrganizationByUuidReturns(Guid uuid, Maybe<Organization> expectedOrg)
        {
            _repositoryMock.Setup(x => x.GetByUuid(uuid)).Returns(expectedOrg);
        }

        private OrganizationRight CreateRight(int organizationId, int userId)
        {
            return new() { Id = A<int>(), OrganizationId = organizationId, UserId = userId };
        }

        private void ExpectAllowModifyReturns(IEntity organization, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(organization)).Returns(value);
        }

        private void ExpectAllowReadReturns(IEntity unit, bool result)
        {
            _authorizationContext.Setup(x => x.AllowReads(unit)).Returns(result);
        }

        private void ExpectAllowCreateReturns<T>(bool value) where T : IEntity
        {
            _authorizationContext.Setup(x => x.AllowCreate<T>(It.IsAny<int>())).Returns(value);
        }

        private void ExpectGetOrganizationByKeyReturns(int organizationId, Organization organization = null)
        {
            _organizationRepository.Setup(x => x.GetByKey(organizationId)).Returns(organization);
        }

        private void ExpectGetOrganizationReadAccessLevelReturns(int organizationId, OrganizationDataReadAccessLevel accessLevel)
        {
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(accessLevel);
        }
    }
}
