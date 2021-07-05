using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Moq;
using Moq.Language.Flow;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationServiceTest : WithAutoFixture
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly OrganizationService _sut;
        private readonly Mock<IOrganizationRoleService> _roleService;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly User _user;
        private readonly Mock<IGenericRepository<Organization>> _organizationRepository;
        private readonly Mock<IGenericRepository<OrganizationRight>> _orgRightRepository;
        private readonly Mock<IGenericRepository<User>> _userRepository;
        private readonly Mock<IOrganizationRepository> _repositoryMock;
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly Mock<IOrgUnitService> _orgUnitServiceMock;

        public OrganizationServiceTest()
        {
            _user = new User() { Id = new Fixture().Create<int>() };
            _authorizationContext = new Mock<IAuthorizationContext>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _roleService = new Mock<IOrganizationRoleService>();
            _transactionManager = new Mock<ITransactionManager>();
            _userContext.Setup(x => x.UserId).Returns(_user.Id);
            _userContext.Setup(x => x.OrganizationIds).Returns(new Fixture().Create<IEnumerable<int>>());
            _organizationRepository = new Mock<IGenericRepository<Organization>>();
            _orgRightRepository = new Mock<IGenericRepository<OrganizationRight>>();
            _userRepository = new Mock<IGenericRepository<User>>();
            _repositoryMock = new Mock<IOrganizationRepository>();
            _orgUnitServiceMock = new Mock<IOrgUnitService>();
            _sut = new OrganizationService(
                _organizationRepository.Object,
                _orgRightRepository.Object,
                _userRepository.Object,
                _authorizationContext.Object,
                _userContext.Object,
                Mock.Of<ILogger>(),
                _roleService.Object,
                _transactionManager.Object,
                _repositoryMock.Object,
                _orgUnitServiceMock.Object);
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
        [InlineData(OrganizationTypeKeys.Kommune, false)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, true)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, false)]
        public void CreateNewOrganization_Returns_Ok(OrganizationTypeKeys organizationType, bool expectRolesAssigned)
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
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
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
            _roleService.Verify(x => x.MakeLocalAdmin(_user, newOrg), Times.Exactly(expectRolesAssigned ? 1 : 0));
            _roleService.Verify(x => x.MakeUser(_user, newOrg), Times.Exactly(expectRolesAssigned ? 1 : 0));
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
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
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
            var matchedRight1 = CreateRight(organizationId, userId);
            var matchedRight2 = CreateRight(organizationId, userId);
            var unmatchedRight1 = CreateRight(A<int>(), userId);
            var unmatchedRight2 = CreateRight(organizationId, A<int>());
            _orgRightRepository.Setup(x => x.AsQueryable()).Returns(new[] { matchedRight1, unmatchedRight1, matchedRight2, unmatchedRight2 }.AsQueryable());

            //Act
            var result = _sut.RemoveUser(organizationId, userId);

            //Assert that only the right entities were removed
            Assert.True(result.Ok);
            _orgRightRepository.Verify(x => x.DeleteByKey(matchedRight1.Id), Times.Once);
            _orgRightRepository.Verify(x => x.DeleteByKey(matchedRight2.Id), Times.Once);
            _orgRightRepository.Verify(x => x.DeleteByKey(unmatchedRight1.Id), Times.Never);
            _orgRightRepository.Verify(x => x.DeleteByKey(unmatchedRight2.Id), Times.Never);
        }

        [Fact]
        public void GetAllOrganizations_Returns_All()
        {
            //Arrange
            var expectedOrg1 = new Organization() { Id = A<int>() };
            var expectedOrg2 = new Organization() { Id = A<int>() };
            var expectedOrg3 = new Organization() { Id = A<int>() };
            _repositoryMock.Setup(x => x.GetAll()).Returns(new List<Organization>() { expectedOrg1, expectedOrg2, expectedOrg3 }.AsQueryable());
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

            var expected1 = new Organization { Id = org1WithMembership, AccessModifier = AccessModifier.Local};
            var expected2 = new Organization { Id = org2WithMembership , AccessModifier = AccessModifier.Public};
            var expected3 = new Organization { Id = org1WithoutMembership ,AccessModifier = AccessModifier.Public};
            var excluded = new Organization { Id = org2WithoutMembership , AccessModifier = AccessModifier.Local};

            var allValues = new[] { expected1, expected2, excluded, expected3 };

            _repositoryMock.Setup(x => x.GetAll()).Returns(allValues.AsQueryable());
            _userContext.Setup(x => x.OrganizationIds).Returns(new[] {org1WithMembership, org2WithMembership});

            //Act
            var organizations = _sut.SearchAccessibleOrganizations().ToList();

            //Assert
            Assert.Equal(allValues.Except(excluded.WrapAsEnumerable()).ToList(), organizations.ToList());
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

        [Fact]
        public void GetOrganization_Returns_Success()
        {
            //Arrange
            var uuid = A<Guid>();
            var expectedOrg = new Organization();
            ExpectGetOrganizationByUuidReturns(uuid, expectedOrg);
            ExpectAllowReadOrganizationReturns(expectedOrg, true);

            //Act
            var result = _sut.GetOrganization(uuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedOrg, result.Value);
        }

        [Fact]
        public void GetOrganization_Returns_Forbidden()
        {
            //Arrange
            var uuid = A<Guid>();
            var expectedOrg = new Organization();
            ExpectGetOrganizationByUuidReturns(uuid, expectedOrg);
            ExpectAllowReadOrganizationReturns(expectedOrg, false);

            //Act
            var result = _sut.GetOrganization(uuid);

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
            var result = _sut.GetOrganization(uuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
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

        private void ExpectAllowCreateReturns<T>(bool value) where T : IEntity
        {
            _authorizationContext.Setup(x => x.AllowCreate<T>(It.IsAny<int>())).Returns(value);
        }

        private void ExpectGetOrganizationByKeyReturns(int organizationId, Organization organization = null)
        {
            _organizationRepository.Setup(x => x.GetByKey(organizationId)).Returns(organization);
        }
    }
}
