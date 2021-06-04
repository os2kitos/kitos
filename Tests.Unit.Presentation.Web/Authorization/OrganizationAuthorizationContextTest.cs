using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class OrganizationAuthorizationContextTest : WithAutoFixture
    {
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly OrganizationAuthorizationContext _sut;
        private readonly Mock<IModuleModificationPolicy> _moduleLevelAccessPolicy;
        private readonly Mock<IGlobalReadAccessPolicy> _globalAccessPolicy;
        private readonly Mock<IModuleCreationPolicy> _creationPolicy;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly int _userId;

        public OrganizationAuthorizationContextTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _userId = new Fixture().Create<int>();
            _userContextMock.Setup(x => x.UserId).Returns(_userId);
            _moduleLevelAccessPolicy = new Mock<IModuleModificationPolicy>();
            _globalAccessPolicy = new Mock<IGlobalReadAccessPolicy>();
            var typeResolver = new Mock<IEntityTypeResolver>();
            typeResolver.Setup(x => x.Resolve(It.IsAny<Type>())).Returns<Type>(t => t);
            _creationPolicy = new Mock<IModuleCreationPolicy>();
            _creationPolicy.Setup(x => x.AllowCreation(It.IsAny<int>(), It.IsAny<Type>())).Returns(true);
            _userRepository = new Mock<IUserRepository>();
            _userRepository.Setup(x => x.GetById(_userId)).Returns(new User { Id = _userId });
            _sut = new OrganizationAuthorizationContext(_userContextMock.Object, typeResolver.Object, _moduleLevelAccessPolicy.Object, _globalAccessPolicy.Object, _creationPolicy.Object, _userRepository.Object);
        }

        [Theory]
        [InlineData(true, false, OrganizationCategory.Other, CrossOrganizationDataReadAccessLevel.All)]
        [InlineData(false, false, OrganizationCategory.Municipality, CrossOrganizationDataReadAccessLevel.Public)]
        [InlineData(false, false, OrganizationCategory.Other, CrossOrganizationDataReadAccessLevel.None)]

        //Regardless of municipality or not, a user marked as rightsholder access will only get rightsholder cross level access rights
        [InlineData(false, true, OrganizationCategory.Other, CrossOrganizationDataReadAccessLevel.RightsHolder)]
        [InlineData(false, true, OrganizationCategory.Municipality, CrossOrganizationDataReadAccessLevel.RightsHolder)]
        public void GetCrossOrganizationReadAccess_Returns_Based_On_Role_And_Organization_Type(bool isGlobalAdmin, bool isRightsHolder, OrganizationCategory organizationCategory, CrossOrganizationDataReadAccessLevel expectedResult)
        {
            //Arrange
            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectUserHasRoleInOrganizationOfType(OrganizationCategory.Municipality, organizationCategory == OrganizationCategory.Municipality);
            ExpectUserHasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess, isRightsHolder);

            //Act
            var result = _sut.GetCrossOrganizationReadAccess();

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, false, false, false, OrganizationDataReadAccessLevel.All)]
        [InlineData(false, false, true, false, OrganizationDataReadAccessLevel.All)]
        [InlineData(false, false, false, true, OrganizationDataReadAccessLevel.Public)]
        [InlineData(false, false, false, false, OrganizationDataReadAccessLevel.None)]

        [InlineData(false, true, false, false, OrganizationDataReadAccessLevel.RightsHolder)]
        [InlineData(false, true, true, false, OrganizationDataReadAccessLevel.All)] //Rightsholders still get full access to own organization data
        [InlineData(false, true, false, true, OrganizationDataReadAccessLevel.RightsHolder)] //Even if municipality rightsholder access is granted
        public void GetOrganizationReadAccessLevel_Returns(bool isGlobalAdmin, bool isRightsHolder, bool isActiveInOrganization, bool isMunicipality, OrganizationDataReadAccessLevel expectedResult)
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectUserHasRoleIn(targetOrganization, isActiveInOrganization);
            ExpectUserHasRoleInOrganizationOfType(OrganizationCategory.Municipality, isMunicipality);
            ExpectUserHasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess, isRightsHolder);

            //Act
            var hasAccess = _sut.GetOrganizationReadAccessLevel(targetOrganization);

            //Assert
            Assert.Equal(expectedResult, hasAccess);
        }


        public interface IRightsHolderOwnedObject : IEntity, IOwnedByOrganization, IHasRightsHolder { }

        [Theory]
        [InlineData(false, false, false, AccessModifier.Local, false)]
        [InlineData(true, false, false, AccessModifier.Local, false)]
        [InlineData(true, true, false, AccessModifier.Local, true)]
        [InlineData(true, true, false, AccessModifier.Public, true)]
        [InlineData(true, false, true, AccessModifier.Public, true)]
        [InlineData(false, false, true, AccessModifier.Public, true)]
        [InlineData(false, false, true, AccessModifier.Local, true)]
        public void AllowReads_For_Context_Dependent_With_RightsHolder_Object_Returns(bool isRightsHolderInAnyOrganization, bool isRightsholderForEntity, bool isInSameOrg, AccessModifier accessModifier, bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            var idOfRightsHoldingOrganization = organizationId + 1;
            var anotherRightsHolder = idOfRightsHoldingOrganization + 1;

            var entityMock = new Mock<IRightsHolderOwnedObject>();
            entityMock.Setup(x => x.OrganizationId).Returns(organizationId);
            entityMock.Setup(x => x.GetRightsHolderOrganizationId()).Returns(isRightsholderForEntity
                ? Maybe<int>.Some(idOfRightsHoldingOrganization)
                : Maybe<int>.Some(anotherRightsHolder));

            var entity = entityMock.Object;

            ExpectUserIsGlobalAdmin(false);
            ExpectHasRoleInSameOrganizationAsReturns(entity, isInSameOrg);
            ExpectUserHasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess, isRightsHolderInAnyOrganization);
            ExpectHasRoleReturns(idOfRightsHoldingOrganization, OrganizationRole.RightsHolderAccess, isRightsholderForEntity);

            //Act
            var result = _sut.AllowReads(entity);

            //Assert
            Assert.Equal(expectedResult, result);
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
            var entity = inputIsActiveUser ? CreateUserEntity(_userId) : CreateTestItSystem(accessModifier);

            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectHasRoleInSameOrganizationAsReturns(entity, isInSameOrg);
            ExpectUserHasRoleInOrganizationOfType(OrganizationCategory.Municipality, isUserActiveInMunicipality);

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
            var inputEntity = inputIsActiveUser ? CreateUserEntity(_userId) : Mock.Of<IEntity>();

            ExpectUserIsGlobalAdmin(isGlobalAdmin);

            //Act
            var result = _sut.AllowReads(inputEntity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void AllowReads_For_GlobalReadableType_Returns_True()
        {
            //Arrange
            var inputEntity = Mock.Of<IEntity>();
            _globalAccessPolicy.Setup(x => x.Allow(inputEntity.GetType())).Returns(true);

            //Act
            var result = _sut.AllowReads(inputEntity);

            //Assert
            Assert.True(result);
        }

        [Theory]
        //Checks not bound to context condition
        [InlineData(true, false, false, false, false, false, true)]
        [InlineData(false, true, false, false, false, false, true)]
        [InlineData(false, false, true, true, false, false, true)]

        //Same organization - positive matches
        [InlineData(false, false, false, true, true, false, true)]

        //Same organization - negative matches
        [InlineData(false, false, false, true, false, false, false)]
        [InlineData(false, false, false, true, false, true, false)]

        //Different organization for context bound object
        [InlineData(false, false, false, false, false, false, false)]
        public void AllowUpdates_For_Context_Dependent_Object_Returns(
            bool isGlobalAdmin,
            bool inputIsActiveUser,
            bool hasAssignedWriteAccess,
            bool isInSameOrganization,
            bool hasModuleLevelAccess,
            bool inputIsAUser,
            bool expectedResult)
        {
            //Arrange
            var inputEntity = inputIsActiveUser || inputIsAUser ? CreateUserEntity(inputIsActiveUser ? _userId : _userId + 1) : CreateTestItSystem(AccessModifier.Public, hasAssignedWriteAccess);

            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectHasRoleInSameOrganizationAsReturns(inputEntity, isInSameOrganization);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);

            //Act
            var allowUpdates = _sut.AllowModify(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, false, false, true)]
        [InlineData(false, true, false, true)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, false, false)]
        public void AllowUpdates_For_Context_Independent_Object_Returns(
           bool isGlobalAdmin,
           bool inputIsActiveUser,
           bool hasModuleLevelAccess,
           bool expectedResult)
        {
            //Arrange
            var inputEntity = inputIsActiveUser ? CreateUserEntity(_userId) : Mock.Of<IEntity>();

            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);

            //Act
            var allowUpdates = _sut.AllowModify(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        public interface ISimpleEntityWithAccessModifier : IEntity, IHasAccessModifier, IOwnedByOrganization { }
        public interface IContractElement : IEntity, IHasAccessModifier, IContractModule, IOwnedByOrganization { }
        public interface IOrganizationElement : IEntity, IHasAccessModifier, IOrganizationModule, IOwnedByOrganization { }

        [Fact]
        public void HasPermission_With_VisibilityControlPermission_Returns_False_If_Input_Does_Not_Have_AccessModifier()
        {
            //Arrange
            var inputEntity = Mock.Of<IEntity>();

            //Act
            var actual = _sut.HasPermission(new VisibilityControlPermission(inputEntity));

            //Assert
            Assert.False(actual);
        }

        private void Test_HasPermission_With_VisibilityControlPermission_Returns<T>(OrganizationRole userRole, bool expectedResult) where T : class, IEntity, IHasAccessModifier, IOwnedByOrganization
        {
            //Arrange
            var organizationId = A<int>();
            var inputEntity = Mock.Of<T>(x => x.OrganizationId == organizationId);

            ExpectUserIsGlobalAdmin(userRole == OrganizationRole.GlobalAdmin);
            ExpectUserHasRoles(organizationId, userRole);

            //Act
            var actual = _sut.HasPermission(new VisibilityControlPermission(inputEntity));

            //Assert
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        public void HasPermission_With_VisibilityControlPermission_Returns_For_SimpleEntityWithAccessModifier(OrganizationRole userRole, bool expectedResult)
        {
            Test_HasPermission_With_VisibilityControlPermission_Returns<ISimpleEntityWithAccessModifier>(userRole, expectedResult);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        public void HasPermission_With_VisibilityControlPermission_Returns_For_IContractElement(OrganizationRole userRole, bool expectedResult)
        {
            Test_HasPermission_With_VisibilityControlPermission_Returns<IContractElement>(userRole, expectedResult);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        public void HasPermission_With_VisibilityControlPermission_Returns_For_IOrganizationElement(OrganizationRole userRole, bool expectedResult)
        {
            Test_HasPermission_With_VisibilityControlPermission_Returns<IOrganizationElement>(userRole, expectedResult);
        }

        [Theory]
        [InlineData(OrganizationTypeKeys.Kommune, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationTypeKeys.Kommune, OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, OrganizationRole.LocalAdmin, true)]
        public void HasPermission_With_DefineOrganizationTypePermission_Returns(OrganizationTypeKeys organizationType, OrganizationRole userRole, bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            ExpectUserHasRoles(organizationId, userRole);
            ExpectUserIsGlobalAdmin(userRole == OrganizationRole.GlobalAdmin);

            //Act
            var actual = _sut.HasPermission(new DefineOrganizationTypePermission(organizationType, organizationId));

            //Assert
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(OrganizationRole.User, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(OrganizationRole.User, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.User, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.User, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.GlobalAdmin, true)]
        public void HasPermission_With_AdministerOrganizationRightPermission_Returns(OrganizationRole administeredRole, OrganizationRole userRole, bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            var right = new OrganizationRight
            {
                Role = administeredRole,
                OrganizationId = organizationId
            };
            ExpectUserIsGlobalAdmin(userRole == OrganizationRole.GlobalAdmin);
            ExpectUserHasRoles(organizationId, userRole);

            //Act
            var actual = _sut.HasPermission(new AdministerOrganizationRightPermission(right));

            //Assert
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_ItSystem_Returns(bool expectedResult)
        {
            Allow_Create_Returns<ItSystem>(expectedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_ItContract_Returns(bool expectedResult)
        {
            Allow_Create_Returns<ItContract>(expectedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_ItSystemUsage_Returns(bool expectedResult)
        {
            Allow_Create_Returns<ItSystemUsage>(expectedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_ItProject_Returns(bool expectedResult)
        {
            Allow_Create_Returns<ItProject>(expectedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_ItInterface_Returns(bool expectedResult)
        {
            Allow_Create_Returns<ItInterface>(expectedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_Organization_Returns(bool expectedResult)
        {
            Allow_Create_Returns<Organization>(expectedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_User_Returns(bool expectedResult)
        {
            Allow_Create_Returns<User>(expectedResult);
        }

        [Theory]
        //Checks not bound to context condition
        [InlineData(true, false, false, false, false, false, true)]
        [InlineData(false, true, false, false, false, false, true)]
        [InlineData(false, false, true, true, false, false, true)]

        //Same organization - positive matches
        [InlineData(false, false, false, true, true, false, true)]

        //Same organization - negative matches
        [InlineData(false, false, false, true, false, false, false)]
        [InlineData(false, false, false, true, false, true, false)]

        //Different organization for context bound object
        [InlineData(false, false, false, false, false, false, false)]
        public void AllowDelete_For_Context_Dependent_Object_Returns(
           bool isGlobalAdmin,
           bool inputIsActiveUser,
           bool hasAssignedWriteAccess,
           bool isInSameOrganization,
           bool hasModuleLevelAccess,
           bool inputIsAUser,
           bool expectedResult)
        {
            //Arrange
            var inputEntity = inputIsActiveUser || inputIsAUser ? CreateUserEntity(inputIsActiveUser ? _userId : _userId + 1) : CreateItProject(AccessModifier.Public, hasAssignedWriteAccess);

            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectHasRoleInSameOrganizationAsReturns(inputEntity, isInSameOrganization);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, false, false, true)]
        [InlineData(false, true, false, true)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, false, false)]
        public void AllowDelete_For_Context_Independent_Object_Returns(
           bool isGlobalAdmin,
           bool inputIsActiveUser,
           bool hasModuleLevelAccess,
           bool expectedResult)
        {
            //Arrange
            var inputEntity = inputIsActiveUser ? CreateUserEntity(_userId) : Mock.Of<IEntity>();

            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AllowDelete_For_ItSystem_Object_Returns(
           bool isGlobalAdmin,
           bool isInSameOrganization,
           bool expectedResult)
        {
            //Arrange
            var inputEntity = CreateTestItSystem(AccessModifier.Public);

            ExpectUserIsGlobalAdmin(isGlobalAdmin);
            ExpectHasRoleInSameOrganizationAsReturns(inputEntity, isInSameOrganization);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void AllowSystemUsageMigration_Returns(bool globalAdmin, bool expectedResult)
        {
            //Arrange
            ExpectUserIsGlobalAdmin(globalAdmin);

            //Act
            var result = _sut.HasPermission(new SystemUsageMigrationPermission());

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, false, false)]
        public void AllowBatchImport_Returns(bool globalAdmin, bool localAdmin, bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            ExpectUserIsGlobalAdmin(globalAdmin);
            ExpectHasRoleReturns(organizationId, OrganizationRole.LocalAdmin, localAdmin);

            //Act
            var result = _sut.HasPermission(new BatchImportPermission(organizationId));

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(OrganizationTypeKeys.Kommune, false, false, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, false, false, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, false, false)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, false, false)]
        [InlineData(OrganizationTypeKeys.Kommune, true, false, true)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, true, false, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, true, false, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, true, false, true)]
        [InlineData(OrganizationTypeKeys.Kommune, false, true, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, false, true, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, true, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, true, true)]
        public void AllowChangeOrganizationType_Returns(OrganizationTypeKeys organizationType, bool globalAdmin, bool localAdmin, bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            ExpectUserIsGlobalAdmin(globalAdmin);
            ExpectHasRoleReturns(organizationId, OrganizationRole.LocalAdmin, localAdmin);

            //Act
            var result = _sut.HasPermission(new DefineOrganizationTypePermission(organizationType, organizationId));

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private void Allow_Create_Returns<T>(bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            _creationPolicy.Setup(x => x.AllowCreation(organizationId, typeof(T))).Returns(expectedResult);

            //Act
            var result = _sut.AllowCreate<T>(organizationId);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private void ExpectHasModuleLevelAccessReturns(IEntity inputEntity, bool hasModuleLevelAccess)
        {
            _moduleLevelAccessPolicy.Setup(x => x.AllowModification(inputEntity)).Returns(hasModuleLevelAccess);
        }

        private ItSystem CreateTestItSystem(AccessModifier accessModifier, bool hasAssignedWriteAccess = false)
        {
            var testItSystem = new ItSystem { AccessModifier = accessModifier };
            if (hasAssignedWriteAccess)
            {
                testItSystem.ObjectOwnerId = _userId;
            }
            return testItSystem;
        }

        private ItProject CreateItProject(AccessModifier accessModifier, bool hasAssignedWriteAccess)
        {
            var itProject = new ItProject
            {
                AccessModifier = accessModifier
            };
            if (hasAssignedWriteAccess)
            {
                itProject.Rights = new List<ItProjectRight>()
                {
                    new ItProjectRight {UserId = _userId,Role = new ItProjectRole {HasWriteAccess = true}},

                };
            }
            return itProject;
        }

        private void ExpectHasRoleInSameOrganizationAsReturns(IEntity entity, bool value)
        {
            _userContextMock.Setup(x => x.HasRoleInSameOrganizationAs(entity)).Returns(value);
        }

        private void ExpectUserHasRoleInOrganizationOfType(OrganizationCategory organizationCategory, bool value)
        {
            _userContextMock.Setup(x => x.HasRoleInOrganizationOfType(organizationCategory)).Returns(value);
        }

        private void ExpectUserIsGlobalAdmin(bool isGlobalAdmin)
        {
            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(isGlobalAdmin);
        }

        private void ExpectUserHasRoleIn(int targetOrganization, bool value)
        {
            _userContextMock.Setup(x => x.HasRoleIn(targetOrganization)).Returns(value);
        }

        private void ExpectHasRoleReturns(int organizationId, OrganizationRole role, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(organizationId, role)).Returns(value);
        }

        private void ExpectUserHasRoles(int organizationId, params OrganizationRole[] targetRoles)
        {
            foreach (var organizationRole in Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>())
            {
                ExpectHasRoleReturns(organizationId, organizationRole, targetRoles.Contains(organizationRole));
            }
        }
        private void ExpectUserHasRoleInAnyOrganization(OrganizationRole role, bool val)
        {
            _userContextMock.Setup(x => x.HasRoleInAnyOrganization(role)).Returns(val);
        }
        private static IEntity CreateUserEntity(int id)
        {
            return (IEntity)new User() { Id = id };
        }
    }
}
