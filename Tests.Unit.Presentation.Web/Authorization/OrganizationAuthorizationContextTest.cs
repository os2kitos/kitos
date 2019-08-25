using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Moq;
using Presentation.Web.Infrastructure.Authorization;
using Presentation.Web.Infrastructure.Authorization.Context;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class OrganizationAuthorizationContextTest : WithAutoFixture
    {
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly OrganizationAuthorizationContext _sut;

        public OrganizationAuthorizationContextTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new OrganizationAuthorizationContext(_userContextMock.Object);
        }

        [Theory]
        [InlineData(true, false, false, true)]
        [InlineData(false, true, false, true)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, false, false)]
        public void AllowReadsWithinOrganization_Returns(bool isGlobalAdmin, bool isActiveInOrganization, bool isMunicipality, bool expectedResult)
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectIsActiveInOrganizationReturns(targetOrganization, isActiveInOrganization);
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, isMunicipality);

            //Act
            var hasAccess = _sut.AllowReadsWithinOrganization(targetOrganization);

            //Assert
            Assert.Equal(expectedResult, hasAccess);
        }

        [Theory]
        [InlineData(true, false, false, false, AccessModifier.Local, true)]
        [InlineData(false, true, false, false, AccessModifier.Local, true)]
        [InlineData(false, false, true, false, AccessModifier.Local, true)]
        [InlineData(false, false, false, true, AccessModifier.Public, true)]
        [InlineData(false, false, false, true, AccessModifier.Local, false)]
        [InlineData(false, false, false, false, AccessModifier.Public, false)]
        public void AllowReads_For_Context_Dependent_Object_Returns(bool isGlobalAdmin, bool inputIsActiveUser, bool isInSameOrg, bool isUserActiveInMunicipality, AccessModifier accessModifier, bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var entity = inputIsActiveUser ? CreateUserEntity(userId) : CreateTestItSystem(accessModifier);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectIsActiveInSameOrganizationAsReturns((IContextAware)entity, isInSameOrg);
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, isUserActiveInMunicipality);

            //Act
            var result = _sut.AllowReads(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void AllowReads_For_Context_Independent_Object_Returns(bool isGlobalAdmin, bool inputIsActiveUser, bool expectedResult)
        {
            //Arrange
            var activeUserId = A<int>();
            var inputEntity = inputIsActiveUser ? CreateUserEntity(activeUserId) : Mock.Of<IEntity>();

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(activeUserId);

            //Act
            var result = _sut.AllowReads(inputEntity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        //Checks not bound to context condition
        [InlineData(true, false, false, false, false, false, false, false, true)]
        [InlineData(false, true, false, false, false, false, false, false, true)]
        [InlineData(false, false, true, false, false, false, false, false, true)]

        //Same organization - positive matches
        [InlineData(false, false, false, true, true, false, false, false, true)]
        [InlineData(false, false, false, true, false, true, false, false, true)]
        [InlineData(false, false, false, true, false, false, false, true, true)]

        //Same organization - negative matches
        [InlineData(false, false, false, true, false, false, false, false, false)]
        [InlineData(false, false, false, true, false, false, true, true, false)]

        //Different organization for context bound object
        [InlineData(false, false, false, false, true, false, false, false, false)]
        public void AllowUpdates_For_Context_Dependent_Object_Returns(
            bool isGlobalAdmin,
            bool inputIsActiveUser,
            bool hasAssignedWriteAccess,
            bool isInSameOrganization,
            bool isLocalAdmin,
            bool hasModuleLevelAccess,
            bool inputIsAUser,
            bool hasOwnership,
            bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var inputEntity = inputIsActiveUser || inputIsAUser ? CreateUserEntity(inputIsActiveUser ? userId : A<int>()) : CreateTestItSystem(AccessModifier.Public);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasAssignedWriteAccessReturns(inputEntity, hasAssignedWriteAccess);
            ExpectIsActiveInSameOrganizationAsReturns((IContextAware)inputEntity, isInSameOrganization);
            ExpectHasRoleReturns(OrganizationRole.LocalAdmin, isLocalAdmin);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);
            ExpectHasOwnershipReturns(inputEntity, hasOwnership);

            //Act
            var allowUpdates = _sut.AllowUpdates(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, false, false, false, false, true)]
        [InlineData(false, true, false, false, false, true)]
        [InlineData(false, false, true, false, false, true)]
        [InlineData(false, false, false, true, false, true)]
        [InlineData(false, false, false, false, true, true)]
        [InlineData(false, false, false, false, false, false)]
        public void AllowUpdates_For_Context_Independent_Object_Returns(
           bool isGlobalAdmin,
           bool inputIsActiveUser,
           bool hasAssignedWriteAccess,
           bool hasModuleLevelAccess,
           bool hasOwnership,
           bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var inputEntity = inputIsActiveUser ? CreateUserEntity(userId) : Mock.Of<IEntity>();

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasAssignedWriteAccessReturns(inputEntity, hasAssignedWriteAccess);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);
            ExpectHasOwnershipReturns(inputEntity, hasOwnership);

            //Act
            var allowUpdates = _sut.AllowUpdates(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        public void AllowEntityVisibilityControl_Returns_True_If_HasWriteAccess_And_Is_AllowedToModifyVisibility(bool isGlobalAdmin, bool isAllowedToChangeVisibility, bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var inputEntity = Mock.Of<IEntity>();

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectCanChangeVisibilityOfReturns(isAllowedToChangeVisibility, inputEntity);

            //Act
            var allowUpdates = _sut.AllowEntityVisibilityControl(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(false, false, false)]
        public void Allow_Create_ItSystem_Returns(bool isGlobalAdmin, bool isReadOnly, bool expectedResult)
        {
            Allow_Create_Returns<ItSystem>(isGlobalAdmin, isReadOnly, expectedResult);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Allow_Create_ItContract_Returns(bool isReadOnly, bool expectedResult)
        {
            Allow_Create_Returns<ItContract>(false, isReadOnly, expectedResult);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Allow_Create_ItSystemUsage_Returns(bool isReadOnly, bool expectedResult)
        {
            Allow_Create_Returns<ItSystemUsage>(false, isReadOnly, expectedResult);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Allow_Create_ItProject_Returns(bool isReadOnly, bool expectedResult)
        {
            Allow_Create_Returns<ItProject>(false, isReadOnly, expectedResult);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Allow_Create_ItInterface_Returns(bool isReadOnly, bool expectedResult)
        {
            Allow_Create_Returns<ItInterface>(false, isReadOnly, expectedResult);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Allow_Create_Organization_Returns(bool isReadOnly, bool expectedResult)
        {
            Allow_Create_Returns<Organization>(false, isReadOnly, expectedResult);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Allow_Create_User_Returns(bool isReadOnly, bool expectedResult)
        {
            Allow_Create_Returns<User>(false, isReadOnly, expectedResult);
        }

        [Theory]
        //Checks not bound to context condition
        [InlineData(true, false, false, false, false, false, false, false, true)]
        [InlineData(false, true, false, false, false, false, false, false, true)]
        [InlineData(false, false, true, false, false, false, false, false, true)]

        //Same organization - positive matches
        [InlineData(false, false, false, true, true, false, false, false, true)]
        [InlineData(false, false, false, true, false, true, false, false, true)]
        [InlineData(false, false, false, true, false, false, false, true, true)]

        //Same organization - negative matches
        [InlineData(false, false, false, true, false, false, false, false, false)]
        [InlineData(false, false, false, true, false, false, true, true, false)]

        //Different organization for context bound object
        [InlineData(false, false, false, false, true, false, false, false, false)]
        public void AllowDelete_For_Context_Dependent_Object_Returns(
           bool isGlobalAdmin,
           bool inputIsActiveUser,
           bool hasAssignedWriteAccess,
           bool isInSameOrganization,
           bool isLocalAdmin,
           bool hasModuleLevelAccess,
           bool inputIsAUser,
           bool hasOwnership,
           bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var inputEntity = inputIsActiveUser || inputIsAUser ? CreateUserEntity(inputIsActiveUser ? userId : A<int>()) : CreateItProject(AccessModifier.Public);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasAssignedWriteAccessReturns(inputEntity, hasAssignedWriteAccess);
            ExpectIsActiveInSameOrganizationAsReturns((IContextAware)inputEntity, isInSameOrganization);
            ExpectHasRoleReturns(OrganizationRole.LocalAdmin, isLocalAdmin);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);
            ExpectHasOwnershipReturns(inputEntity, hasOwnership);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, false, false, false, false, true)]
        [InlineData(false, true, false, false, false, true)]
        [InlineData(false, false, true, false, false, true)]
        [InlineData(false, false, false, true, false, true)]
        [InlineData(false, false, false, false, true, true)]
        [InlineData(false, false, false, false, false, false)]
        public void AllowDelete_For_Context_Independent_Object_Returns(
           bool isGlobalAdmin,
           bool inputIsActiveUser,
           bool hasAssignedWriteAccess,
           bool hasModuleLevelAccess,
           bool hasOwnership,
           bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var inputEntity = inputIsActiveUser ? CreateUserEntity(userId) : Mock.Of<IEntity>();

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasAssignedWriteAccessReturns(inputEntity, hasAssignedWriteAccess);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);
            ExpectHasOwnershipReturns(inputEntity, hasOwnership);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(false, true, false, false, true)]
        [InlineData(false, false, true, true, true)]
        [InlineData(true, true, false, false, false)]
        [InlineData(true, false, true, true, false)]
        [InlineData(false, false, false, true, false)]
        [InlineData(false, false, true, false, false)]
        public void AllowDelete_For_ItSystem_Object_Returns(
           bool isReadOnly,
           bool isGlobalAdmin,
           bool isInSameOrganization,
           bool isLocalAdmin,
           bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var inputEntity = CreateTestItSystem(AccessModifier.Public);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectHasRoleReturns(OrganizationRole.ReadOnly, isReadOnly);
            ExpectGetUserIdReturns(userId);
            ExpectIsActiveInSameOrganizationAsReturns((IContextAware)inputEntity, isInSameOrganization);
            ExpectHasRoleReturns(OrganizationRole.LocalAdmin, isLocalAdmin);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        private void Allow_Create_Returns<T>(bool isGlobalAdmin, bool isReadOnly, bool expectedResult)
        {
            //Arrange
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectHasRoleReturns(OrganizationRole.ReadOnly, isReadOnly);

            //Act
            var result = _sut.AllowCreate<T>();

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private void ExpectCanChangeVisibilityOfReturns(bool isAllowedToChangeVisibility, IEntity inputEntity)
        {
            _userContextMock.Setup(x => x.CanChangeVisibilityOf(inputEntity)).Returns(isAllowedToChangeVisibility);
        }

        private void ExpectHasOwnershipReturns(IEntity inputEntity, bool value)
        {
            _userContextMock.Setup(x => x.HasOwnership(inputEntity)).Returns(value);
        }

        private void ExpectHasModuleLevelAccessReturns(IEntity inputEntity, bool hasModuleLevelAccess)
        {
            _userContextMock.Setup(x => x.HasModuleLevelAccessTo(inputEntity)).Returns(hasModuleLevelAccess);
        }

        private void ExpectHasAssignedWriteAccessReturns(IEntity inputEntity, bool value)
        {
            _userContextMock.Setup(x => x.HasAssignedWriteAccess(inputEntity)).Returns(value);
        }

        private static ItSystem CreateTestItSystem(AccessModifier accessModifier)
        {
            return new ItSystem { AccessModifier = accessModifier };
        }

        private static ItProject CreateItProject(AccessModifier accessModifier)
        {
            return new ItProject { AccessModifier = accessModifier };
        }

        private void ExpectIsActiveInSameOrganizationAsReturns(IContextAware entity, bool value)
        {
            _userContextMock.Setup(x => x.IsActiveInSameOrganizationAs(entity)).Returns(value);
        }

        private void ExpectGetUserIdReturns(int userId)
        {
            _userContextMock.Setup(x => x.UserId).Returns(userId);
        }

        private void ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory organizationCategory, bool value)
        {
            _userContextMock.Setup(x => x.IsActiveInOrganizationOfType(organizationCategory)).Returns(value);
        }

        private void ExpectIsActiveInOrganizationReturns(int targetOrganization, bool value)
        {
            _userContextMock.Setup(x => x.IsActiveInOrganization(targetOrganization)).Returns(value);
        }

        private void ExpectHasRoleReturns(OrganizationRole role, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(role)).Returns(value);
        }

        private static IEntity CreateUserEntity(int id)
        {
            return (IEntity)new User() { Id = id };
        }
    }
}
