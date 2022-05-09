﻿using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Moq;
using Tests.Toolkit.Patterns;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class ModuleModificationPolicyTest : WithAutoFixture
    {
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly ModuleModificationPolicy _sut;

        public ModuleModificationPolicyTest()
        {
            _userContext = new Mock<IOrganizationalUserContext>();
            _sut = new ModuleModificationPolicy(_userContext.Object);
        }

        public interface IRightsHolderElement : IEntity, IHasRightsHolder { }
        public interface IContractElement : IEntity, IContractModule { }
        public interface IOrganizationElement : IEntity, IOrganizationModule { }
        public interface IProjectElement : IEntity, IProjectModule { }
        public interface ISystemElement : IEntity, ISystemModule { }
        public interface ICrossCuttingElement : IEntity, ISystemModule, IProjectModule, IContractModule, IOrganizationModule { }

        [Theory]
        [InlineData(typeof(IEntity), true, true, null, false)]//Unknown entity type always returns false from this policy
        [InlineData(typeof(IContractElement), true, false, null, true)]
        [InlineData(typeof(IContractElement), false, true, null, true)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.ContractModuleAdmin, true)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), true, false, null, true)]
        [InlineData(typeof(IOrganizationElement), false, true, null, true)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(typeof(IProjectElement), true, false, null, true)]
        [InlineData(typeof(IProjectElement), false, true, null, true)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ProjectModuleAdmin, true)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), true, false, null, true)]
        [InlineData(typeof(ISystemElement), false, true, null, true)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.SystemModuleAdmin, true)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ICrossCuttingElement), true, false, null, true)]
        [InlineData(typeof(ICrossCuttingElement), false, true, null, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.ContractModuleAdmin, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.SystemModuleAdmin, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.ProjectModuleAdmin, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.OrganizationModuleAdmin, true)]
        public void Allow_Modify_With_Entity_Returns(Type entityType, bool isLocalAdmin, bool isGlobalAdmin, OrganizationRole? otherRole, bool expectedResult)
        {
            //Arrange
            var orgId = A<int>();
            _userContext.Setup(x => x.OrganizationIds).Returns(new[] { orgId });
            SetupUserContext(orgId, isLocalAdmin, isGlobalAdmin, otherRole);

            var entity = (IEntity)MoqTools.MockedObjectFrom(entityType);

            //Act
            var allow = _sut.AllowModification(entity);

            //Assert
            Assert.Equal(expectedResult, allow);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        public void Allow_Modify_With_RightsHolder_Type_Returns(bool hasRightsholderSet, bool isRightsholder, bool expectedResult)
        {
            //Arrange
            var orgId = A<int>();
            _userContext.Setup(x => x.OrganizationIds).Returns(new[] { orgId });

            var entity = new Mock<IRightsHolderElement>();
            var rightsHolderOrg = A<int>();
            entity
                .Setup(x => x.GetRightsHolderOrganizationId())
                .Returns(hasRightsholderSet ? Maybe<int>.Some(rightsHolderOrg) : Maybe<int>.None);
            _userContext
                .Setup(x => x.HasRole(rightsHolderOrg, OrganizationRole.RightsHolderAccess))
                .Returns(isRightsholder);


            //Act
            var allow = _sut.AllowModification(entity.Object);

            //Assert
            Assert.Equal(expectedResult, allow);
        }

        [Theory]
        // UNKNOWN ENTITY FALLBACK
        [InlineData(typeof(Entity), false, false, null, true)]//Unrestricted entity type always returns true from the creation perspective
        // GLOBAL ADMIN
        [InlineData(typeof(ItSystem), false, true, null, true)]
        [InlineData(typeof(ItInterface), false, true, null, true)]
        [InlineData(typeof(ItSystemUsage), false, true, null, true)]
        [InlineData(typeof(ItProject), false, true, null, true)]
        [InlineData(typeof(ItContract), false, true, null, true)]
        [InlineData(typeof(Organization), false, true, null, true)]
        [InlineData(typeof(User), false, true, null, true)]
        // LOCAL ADMIN
        [InlineData(typeof(ItSystem), true, false, null, false)]
        [InlineData(typeof(ItInterface), true, false, null, true)]
        [InlineData(typeof(ItSystemUsage), true, false, null, true)]
        [InlineData(typeof(ItProject), true, false, null, true)]
        [InlineData(typeof(ItContract), true, false, null, true)]
        [InlineData(typeof(User), true, false, null, true)]
        [InlineData(typeof(Organization), true, false, null, false)]
        // SYSTEM ADMIN
        [InlineData(typeof(ItSystem), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(ItInterface), false, false, OrganizationRole.SystemModuleAdmin, true)]
        [InlineData(typeof(ItSystemUsage), false, false, OrganizationRole.SystemModuleAdmin, true)]
        [InlineData(typeof(ItProject), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(ItContract), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(Organization), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(User), false, false, OrganizationRole.SystemModuleAdmin, false)]
        // PROJECT ADMIN
        [InlineData(typeof(ItSystem), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(ItInterface), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(ItSystemUsage), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(ItProject), false, false, OrganizationRole.ProjectModuleAdmin, true)]
        [InlineData(typeof(ItContract), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(Organization), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(User), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        // CONTRACT ADMIN
        [InlineData(typeof(ItSystem), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ItInterface), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ItSystemUsage), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ItProject), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ItContract), false, false, OrganizationRole.ContractModuleAdmin, true)]
        [InlineData(typeof(Organization), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(User), false, false, OrganizationRole.ContractModuleAdmin, false)]
        // ORGANIZATION ADMIN
        [InlineData(typeof(ItSystem), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ItInterface), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ItSystemUsage), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ItProject), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ItContract), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(Organization), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(User), false, false, OrganizationRole.OrganizationModuleAdmin, true)]

        public void Allow_Creation_With_Type_Returns(Type entityType, bool isLocalAdmin, bool isGlobalAdmin, OrganizationRole? otherRole, bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            SetupUserContext(organizationId, isLocalAdmin, isGlobalAdmin, otherRole);

            //Act
            var allow = _sut.AllowCreation(organizationId, entityType);

            //Assert
            Assert.Equal(expectedResult, allow);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Creation_With_Rightsholder_Type_Returns(bool isRightsholder)
        {
            //Arrange
            var organizationId = A<int>();
            if (isRightsholder)
            {
                ExpectUserHasRole(organizationId, OrganizationRole.RightsHolderAccess);
            }

            //Act
            var allow = _sut.AllowCreation(organizationId, typeof(IRightsHolderElement));

            //Assert
            Assert.Equal(isRightsholder, allow);
        }

        private void ExpectUserHasRole(int organizationId, OrganizationRole organizationRole)
        {
            _userContext.Setup(x => x.HasRole(organizationId, organizationRole)).Returns(true);
        }

        private void SetupUserContext(int organizationId, bool isLocalAdmin, bool isGlobalAdmin, OrganizationRole? otherRole)
        {
            if (isLocalAdmin)
                ExpectUserHasRole(organizationId, OrganizationRole.LocalAdmin);

            if (isGlobalAdmin)
            {
                ExpectUserHasRole(organizationId, OrganizationRole.GlobalAdmin);
                _userContext.Setup(x => x.IsGlobalAdmin()).Returns(true);
            }

            if (otherRole.HasValue)
                ExpectUserHasRole(organizationId, otherRole.Value);
        }
    }
}
