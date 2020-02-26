using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Tests.Unit.Presentation.Web.Helpers;
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

        public OrganizationAuthorizationContextTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _moduleLevelAccessPolicy = new Mock<IModuleModificationPolicy>();
            _globalAccessPolicy = new Mock<IGlobalReadAccessPolicy>();
            var typeResolver = new Mock<IEntityTypeResolver>();
            typeResolver.Setup(x => x.Resolve(It.IsAny<Type>())).Returns<Type>(t => t);
            _creationPolicy = new Mock<IModuleCreationPolicy>();
            _creationPolicy.Setup(x => x.AllowCreation(It.IsAny<Type>())).Returns(true);
            _sut = new OrganizationAuthorizationContext(_userContextMock.Object, typeResolver.Object,_moduleLevelAccessPolicy.Object, _globalAccessPolicy.Object,_creationPolicy.Object);
        }

        [Theory]
        [InlineData(true, OrganizationCategory.Other, CrossOrganizationDataReadAccessLevel.All)]
        [InlineData(false, OrganizationCategory.Municipality, CrossOrganizationDataReadAccessLevel.Public)]
        [InlineData(false, OrganizationCategory.Other, CrossOrganizationDataReadAccessLevel.None)]
        public void GetCrossOrganizationReadAccess_Returns_Based_On_Role_And_Organization_Type(bool isGlobalAdmin, OrganizationCategory organizationCategory, CrossOrganizationDataReadAccessLevel expectedResult)
        {
            //Arrange
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, organizationCategory == OrganizationCategory.Municipality);

            //Act
            var result = _sut.GetCrossOrganizationReadAccess();

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, false, false, OrganizationDataReadAccessLevel.All)]
        [InlineData(false, true, false, OrganizationDataReadAccessLevel.All)]
        [InlineData(false, false, true, OrganizationDataReadAccessLevel.Public)]
        [InlineData(false, false, false, OrganizationDataReadAccessLevel.None)]
        public void GetOrganizationReadAccessLevel_Returns(bool isGlobalAdmin, bool isActiveInOrganization, bool isMunicipality, OrganizationDataReadAccessLevel expectedResult)
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectIsActiveInOrganizationReturns(targetOrganization, isActiveInOrganization);
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, isMunicipality);

            //Act
            var hasAccess = _sut.GetOrganizationReadAccessLevel(targetOrganization);

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
            ExpectIsActiveInSameOrganizationAsReturns(entity, isInSameOrg);
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
            var userId = A<int>();
            var inputEntity = inputIsActiveUser || inputIsAUser ? CreateUserEntity(inputIsActiveUser ? userId : A<int>()) : CreateTestItSystem(AccessModifier.Public);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasAssignedWriteAccessReturns(inputEntity, hasAssignedWriteAccess);
            ExpectIsActiveInSameOrganizationAsReturns(inputEntity, isInSameOrganization);
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
            var userId = A<int>();
            var inputEntity = inputIsActiveUser ? CreateUserEntity(userId) : Mock.Of<IEntity>();

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);

            //Act
            var allowUpdates = _sut.AllowModify(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        public interface ISimpleEntityWithAccessModifier : IEntity, IHasAccessModifier { }
        public interface IContractElement : IEntity, IHasAccessModifier, IContractModule { }
        public interface IOrganizationElement : IEntity, IHasAccessModifier, IOrganizationModule { }

        [Theory]
        [InlineData(typeof(IEntity), OrganizationRole.GlobalAdmin, false)] //Type does not allow access modification, so false regardless of roles
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.GlobalAdmin, true)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.LocalAdmin, false)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.User, false)]
        [InlineData(typeof(ISimpleEntityWithAccessModifier), OrganizationRole.ReadOnly, false)]
        [InlineData(typeof(IContractElement), OrganizationRole.GlobalAdmin, true)]
        [InlineData(typeof(IContractElement), OrganizationRole.LocalAdmin, true)]
        [InlineData(typeof(IContractElement), OrganizationRole.ContractModuleAdmin, true)]
        [InlineData(typeof(IContractElement), OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(IContractElement), OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IContractElement), OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IContractElement), OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(IContractElement), OrganizationRole.User, false)]
        [InlineData(typeof(IContractElement), OrganizationRole.ReadOnly, false)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.GlobalAdmin, true)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.LocalAdmin, true)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.User, false)]
        [InlineData(typeof(IOrganizationElement), OrganizationRole.ReadOnly, false)]
        public void HasPermission_With_VisibilityControlPermission_Returns(Type entityType, OrganizationRole userRole, bool expectedResult)
        {
            //Arrange
            var inputEntity = (IEntity)MoqTools.MockOf(entityType);
            ExpectUserHasRoles(userRole);

            //Act
            var actual = _sut.HasPermission(new VisibilityControlPermission(inputEntity));

            //Assert
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(OrganizationTypeKeys.Kommune, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationTypeKeys.Kommune, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationTypeKeys.Kommune, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationTypeKeys.Kommune, OrganizationRole.LocalAdmin, false, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, OrganizationRole.LocalAdmin, false, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationTypeKeys.Virksomhed, OrganizationRole.LocalAdmin, false, true)]
        public void HasPermission_With_DefineOrganizationTypePermission_Returns(OrganizationTypeKeys organizationType, OrganizationRole userRole, bool readOnly, bool expectedResult)
        {
            //Arrange
            if (readOnly)
                ExpectUserHasRoles(userRole, OrganizationRole.ReadOnly);
            else
                ExpectUserHasRoles(userRole);

            //Act
            var actual = _sut.HasPermission(new DefineOrganizationTypePermission(organizationType));

            //Assert
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(OrganizationRole.User, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.OrganizationModuleAdmin, false, true)]
        [InlineData(OrganizationRole.User, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationRole.User, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.User, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.User, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.OrganizationModuleAdmin, false, true)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.ReportModuleAdmin, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.OrganizationModuleAdmin, false, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.OrganizationModuleAdmin, false, true)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.ProjectModuleAdmin, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.OrganizationModuleAdmin, false, true)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.SystemModuleAdmin, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.OrganizationModuleAdmin, false, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, false, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.LocalAdmin, false, true)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.User, false, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.ReportModuleAdmin, false, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.ContractModuleAdmin, false, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.ProjectModuleAdmin, false, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.SystemModuleAdmin, false, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.OrganizationModuleAdmin, false, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.OrganizationModuleAdmin, true, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, false, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.GlobalAdmin, false, true)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationRole.GlobalAdmin, true, true)]
        public void HasPermission_With_AdministerOrganizationRightPermission_Returns(OrganizationRole administeredRole, OrganizationRole userRole, bool readOnly, bool expectedResult)
        {
            //Arrange
            if (readOnly)
                ExpectUserHasRoles(userRole, OrganizationRole.ReadOnly);
            else
                ExpectUserHasRoles(userRole);

            //Act
            var actual = _sut.HasPermission(new AdministerOrganizationRightPermission(new OrganizationRight() { Role = administeredRole }));

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
        [InlineData(false, false, true, true,  false, false, true)]

        //Same organization - positive matches
        [InlineData(false, false, false, true,  true, false, true)]

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
            var userId = A<int>();
            var inputEntity = inputIsActiveUser || inputIsAUser ? CreateUserEntity(inputIsActiveUser ? userId : A<int>()) : CreateItProject(AccessModifier.Public);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasAssignedWriteAccessReturns(inputEntity, hasAssignedWriteAccess);
            ExpectIsActiveInSameOrganizationAsReturns(inputEntity, isInSameOrganization);
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
            var userId = A<int>();
            var inputEntity = inputIsActiveUser ? CreateUserEntity(userId) : Mock.Of<IEntity>();

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserIdReturns(userId);
            ExpectHasModuleLevelAccessReturns(inputEntity, hasModuleLevelAccess);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(false, true, false, true)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, false)]
        [InlineData(false, false, false, false)]
        [InlineData(false, false, true, false)]
        public void AllowDelete_For_ItSystem_Object_Returns(
           bool isReadOnly,
           bool isGlobalAdmin,
           bool isInSameOrganization,
           bool expectedResult)
        {
            //Arrange
            var userId = A<int>();
            var inputEntity = CreateTestItSystem(AccessModifier.Public);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectHasRoleReturns(OrganizationRole.ReadOnly, isReadOnly);
            ExpectGetUserIdReturns(userId);
            ExpectIsActiveInSameOrganizationAsReturns(inputEntity, isInSameOrganization);

            //Act
            var allowUpdates = _sut.AllowDelete(inputEntity);

            //Assert
            Assert.Equal(expectedResult, allowUpdates);
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)] //Global admin cannot be locally read-only
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AllowSystemUsageMigration_Returns(bool globalAdmin, bool readOnly, bool expectedResult)
        {
            //Arrange
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, globalAdmin);
            ExpectHasRoleReturns(OrganizationRole.ReadOnly, readOnly);

            //Act
            var result = _sut.HasPermission(new SystemUsageMigrationPermission());

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(false, true, true, false)]
        [InlineData(false, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, false, false, false)]
        public void AllowBatchImport_Returns(bool globalAdmin, bool localAdmin, bool readOnly, bool expectedResult)
        {
            //Arrange
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, globalAdmin);
            ExpectHasRoleReturns(OrganizationRole.LocalAdmin, localAdmin);
            ExpectHasRoleReturns(OrganizationRole.ReadOnly, readOnly);

            //Act
            var result = _sut.HasPermission(new BatchImportPermission());

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(OrganizationTypeKeys.Kommune, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.Kommune, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.Kommune, false, true, false, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, false, true, false, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, true, false, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, true, false, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, true, true, false)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, true, true, false)]
        public void AllowCreateOrganizationOfType_Returns(
            OrganizationTypeKeys organizationType, 
            bool globalAdmin, 
            bool localAdmin, 
            bool readOnly, 
            bool expectedResult)
        {
            //Arrange
            var organization = new Organization { TypeId = (int)organizationType };

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, globalAdmin);
            ExpectHasRoleReturns(OrganizationRole.LocalAdmin, localAdmin);
            ExpectIsActiveInSameOrganizationAsReturns(organization, localAdmin); //local admin test - always in same org in this scope
            ExpectHasRoleReturns(OrganizationRole.ReadOnly, readOnly);
            _moduleLevelAccessPolicy.Setup(x => x.AllowModification(organization)).Returns(localAdmin);

            //Act
            var result = _sut.AllowCreate<Organization>(organization);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(OrganizationTypeKeys.Kommune, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, false, false, false)]
        [InlineData(OrganizationTypeKeys.Kommune, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, true, false, false, true)]
        [InlineData(OrganizationTypeKeys.Kommune, false, true, false, false)]
        [InlineData(OrganizationTypeKeys.AndenOffentligMyndighed, false, true, false, false)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, true, false, true)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, true, false, true)]
        [InlineData(OrganizationTypeKeys.Interessefællesskab, false, true, true, false)]
        [InlineData(OrganizationTypeKeys.Virksomhed, false, true, true, false)]
        public void AllowChangeOrganizationType_Returns(OrganizationTypeKeys organizationType, bool globalAdmin, bool localAdmin, bool readOnly, bool expectedResult)
        {
            //Arrange
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, globalAdmin);
            ExpectHasRoleReturns(OrganizationRole.LocalAdmin, localAdmin);
            ExpectHasRoleReturns(OrganizationRole.ReadOnly, readOnly);

            //Act
            var result = _sut.HasPermission(new DefineOrganizationTypePermission(organizationType));

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private void Allow_Create_Returns<T>(bool expectedResult)
        {
            //Arrange
            _creationPolicy.Setup(x => x.AllowCreation(typeof(T))).Returns(expectedResult);

            //Act
            var result = _sut.AllowCreate<T>();

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private void ExpectHasModuleLevelAccessReturns(IEntity inputEntity, bool hasModuleLevelAccess)
        {
            _moduleLevelAccessPolicy.Setup(x => x.AllowModification(inputEntity)).Returns(hasModuleLevelAccess);
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

        private void ExpectIsActiveInSameOrganizationAsReturns(IEntity entity, bool value)
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

        private void ExpectUserHasRoles(params OrganizationRole[] targetRoles)
        {
            foreach (var organizationRole in Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>())
            {
                ExpectHasRoleReturns(organizationRole, targetRoles.Contains(organizationRole));
            }
        }

        private static IEntity CreateUserEntity(int id)
        {
            return (IEntity)new User() { Id = id };
        }
    }
}
