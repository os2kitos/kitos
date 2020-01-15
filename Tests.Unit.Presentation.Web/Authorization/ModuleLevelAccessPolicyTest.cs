using System;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class ModuleLevelAccessPolicyTest
    {
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly ModuleLevelAccessPolicy _sut;

        public ModuleLevelAccessPolicyTest()
        {
            _userContext = new Mock<IOrganizationalUserContext>();
            _sut = new ModuleLevelAccessPolicy(_userContext.Object);
        }

        public interface IContractElement : IEntity, IContractModule { }
        public interface IOrganizationElement : IEntity, IOrganizationModule { }
        public interface IProjectElement : IEntity, IProjectModule { }
        public interface IReportElement : IEntity, IReportModule { }
        public interface ISystemElement : IEntity, ISystemModule { }

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
        [InlineData(typeof(IContractElement), false, false, OrganizationRole.ReadOnly, false)]
        [InlineData(typeof(IOrganizationElement), true, false, null, true)]
        [InlineData(typeof(IOrganizationElement), false, true, null, true)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.OrganizationModuleAdmin, true)]
        [InlineData(typeof(IOrganizationElement), false, false, OrganizationRole.ReadOnly, false)]
        [InlineData(typeof(IProjectElement), true, false, null, true)]
        [InlineData(typeof(IProjectElement), false, true, null, true)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ProjectModuleAdmin, true)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(IProjectElement), false, false, OrganizationRole.ReadOnly, false)]
        [InlineData(typeof(ISystemElement), true, false, null, true)]
        [InlineData(typeof(ISystemElement), false, true, null, true)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ReportModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.SystemModuleAdmin, true)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(ISystemElement), false, false, OrganizationRole.ReadOnly, false)]
        [InlineData(typeof(IReportElement), true, false, null, true)]
        [InlineData(typeof(IReportElement), false, true, null, true)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.User, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.ReportModuleAdmin, true)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.ProjectModuleAdmin, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(typeof(IReportElement), false, false, OrganizationRole.ReadOnly, false)]
        public void Allow_Returns(Type entityType, bool isLocalAdmin, bool isGlobalAdmin, OrganizationRole? otherRole, bool expectedResult)
        {
            //Arrange
            if (isLocalAdmin)
                ExpectUserHasRole(OrganizationRole.LocalAdmin);
            if (isGlobalAdmin)
                ExpectUserHasRole(OrganizationRole.GlobalAdmin);
            if (otherRole.HasValue)
                ExpectUserHasRole(otherRole.Value);

            var entity = (IEntity)MoqTools.MockOf(entityType);

            //Act
            var allow = _sut.Allow(entity);

            //Assert
            Assert.Equal(expectedResult, allow);
        }

        private void ExpectUserHasRole(OrganizationRole organizationRole)
        {
            _userContext.Setup(x => x.HasRole(organizationRole)).Returns(true);
        }
    }
}
