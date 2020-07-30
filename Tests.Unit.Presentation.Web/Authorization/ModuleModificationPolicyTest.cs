using System;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class ModuleModificationPolicyTest
    {
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly ModuleModificationPolicy _sut;

        public ModuleModificationPolicyTest()
        {
            _userContext = new Mock<IOrganizationalUserContext>();
            _sut = new ModuleModificationPolicy(_userContext.Object,false);
        }

        public interface IContractElement : IEntity, IContractModule { }
        public interface IOrganizationElement : IEntity, IOrganizationModule { }
        public interface IProjectElement : IEntity, IProjectModule { }
        public interface IReportElement : IEntity, IReportModule { }
        public interface ISystemElement : IEntity, ISystemModule { }
        public interface ICrossCuttingElement : IEntity, ISystemModule, IProjectModule, IContractModule, IReportModule, IOrganizationModule { }

        [Theory]
        [InlineData(typeof(IEntity), true, true, null, false)]//Unknown entity type always returns false from this policy
        [InlineData(typeof(IContractElement), true, false, null, true)]
        [InlineData(typeof(IContractElement), false, true, null, true)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.ContractModuleAdmin, true)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), true, false, null, true)]
        [InlineData(typeof(IOrganizationElement), false, true, null, true)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(typeof(IProjectElement), true, false, null, true)]
        [InlineData(typeof(IProjectElement), false, true, null, true)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ProjectModuleAdmin, true)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), true, false, null, true)]
        [InlineData(typeof(ISystemElement), false, true, null, true)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.SystemModuleAdmin, true)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(IReportElement), true, false, null, true)]
        [InlineData(typeof(IReportElement), false, true, null, true)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.ReportModuleAdmin, true)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ICrossCuttingElement), true, false, null, true)]
        [InlineData(typeof(ICrossCuttingElement), false, true, null, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.ContractModuleAdmin, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.ReportModuleAdmin, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.SystemModuleAdmin, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.ProjectModuleAdmin, true)]
        [InlineData(typeof(ICrossCuttingElement), false, false, OrganizationRole.OrganizationModuleAdmin, true)]
        public void Allow_With_Entity_Returns(Type entityType, bool isLocalAdmin, bool isGlobalAdmin, OrganizationRole? otherRole, bool expectedResult)
        {
            //Arrange
            SetupUserContext(isLocalAdmin, isGlobalAdmin, otherRole);

            var entity = (IEntity)MoqTools.MockOf(entityType);

            //Act
            var allow = _sut.AllowModification(entity);

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
        [InlineData(typeof(ItInterface), true, false, null, false)]
        [InlineData(typeof(ItSystemUsage), true, false, null, true)]
        [InlineData(typeof(ItProject), true, false, null, true)]
        [InlineData(typeof(ItContract), true, false, null, true)]
        [InlineData(typeof(Organization), true, false, null, true)]
        [InlineData(typeof(User), true, false, null, true)]
        // SYSTEM ADMIN
        [InlineData(typeof(ItSystem), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(ItInterface), false, false, OrganizationRole.SystemModuleAdmin, false)]
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

        public void Allow_With_Type_Returns(Type entityType, bool isLocalAdmin, bool isGlobalAdmin, OrganizationRole? otherRole, bool expectedResult)
        {
            //Arrange
            SetupUserContext(isLocalAdmin, isGlobalAdmin, otherRole);

            //Act
            var allow = _sut.AllowCreation(entityType);

            //Assert
            Assert.Equal(expectedResult, allow);
        }

        private void ExpectUserHasRole(OrganizationRole organizationRole)
        {
            _userContext.Setup(x => x.HasRole(organizationRole)).Returns(true);
        }

        private void SetupUserContext(bool isLocalAdmin, bool isGlobalAdmin, OrganizationRole? otherRole)
        {
            if (isLocalAdmin)
                ExpectUserHasRole(OrganizationRole.LocalAdmin);
            if (isGlobalAdmin)
                ExpectUserHasRole(OrganizationRole.GlobalAdmin);
            if (otherRole.HasValue)
                ExpectUserHasRole(otherRole.Value);
        }
    }
}
