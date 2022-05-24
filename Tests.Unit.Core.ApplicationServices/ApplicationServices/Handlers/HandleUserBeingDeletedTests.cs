using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.UIConfiguration.Handlers;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.SSO;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.Role;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Handlers
{
    public class HandleUserBeingDeletedTests : WithAutoFixture
    {
        private readonly Mock<IDataProcessingRegistrationApplicationService> _dprApplicationServiceMock;
        private readonly Mock<IOrganizationRightsService> _organizationRightsServiceMock;
        private readonly Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>> _itContractRightServiceMock;
        private readonly Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>> _itSystemRightServiceMock;
        private readonly Mock<IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject>> _itProjectRightServiceMock;
        private readonly Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>> _organizationUnitRightServiceMock;
        private readonly Mock<ISsoUserIdentityRepository> _ssoUserIdentityRepository;
        private readonly HandleUserBeingDeleted _sut;

        public HandleUserBeingDeletedTests()
        {
            _dprApplicationServiceMock = new Mock<IDataProcessingRegistrationApplicationService>();
            _organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            _itContractRightServiceMock = new Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>>();
            _itSystemRightServiceMock = new Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>>();
            _itProjectRightServiceMock = new Mock<IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject>>();
            _organizationUnitRightServiceMock = new Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>>();
            _ssoUserIdentityRepository = new Mock<ISsoUserIdentityRepository>();

            _sut = new HandleUserBeingDeleted(_dprApplicationServiceMock.Object,
                _organizationRightsServiceMock.Object,
                _itContractRightServiceMock.Object,
                _itSystemRightServiceMock.Object,
                _itProjectRightServiceMock.Object,
                _organizationUnitRightServiceMock.Object,
                _ssoUserIdentityRepository.Object);
        }

        [Fact]
        public void Handle_Deletes_All_User_Rights()
        {
            var roleId = A<int>();
            var organizationRole = A<OrganizationRole>();
            var user = SetupUser(roleId, organizationRole);

            _sut.Handle(new EntityBeingDeletedEvent<User>(user));

            _dprApplicationServiceMock.Verify(x => x.RemoveRole(It.IsAny<int>(), roleId, user.Id), Times.AtLeastOnce);
            Assert.Empty(user.DataProcessingRegistrationRights);

            _organizationRightsServiceMock.Verify(x => x.RemoveRole(It.IsAny<int>(), user.Id, organizationRole), Times.AtLeastOnce);
            Assert.Empty(user.OrganizationRights);

            _itContractRightServiceMock.Verify(x => x.RemoveRole(It.IsAny<ItContract>(), roleId, user.Id), Times.AtLeastOnce);
            Assert.Empty(user.ItContractRights);

            _itSystemRightServiceMock.Verify(x => x.RemoveRole(It.IsAny<ItSystemUsage>(), roleId, user.Id), Times.AtLeastOnce);
            Assert.Empty(user.ItSystemRights);

            _itProjectRightServiceMock.Verify(x => x.RemoveRole(It.IsAny<ItProject>(), roleId, user.Id), Times.AtLeastOnce);
            Assert.Empty(user.ItProjectRights);

            _organizationUnitRightServiceMock.Verify(x => x.RemoveRole(It.IsAny<OrganizationUnit>(), roleId, user.Id), Times.AtLeastOnce);
            Assert.Empty(user.OrganizationUnitRights);

            _ssoUserIdentityRepository.Verify(x => x.DeleteIdentitiesForUser(user.SsoIdentities), Times.AtLeastOnce);
            Assert.Empty(user.SsoIdentities);
        }

        private void ExpectDprApplicationRemoveRoleReturns(int objectId, int roleId, int userId, Result<DataProcessingRegistrationRight, OperationError> result)
        {
            _dprApplicationServiceMock.Setup(x => x.RemoveRole(objectId, roleId, userId)).Returns(result);
        }

        private void ExpectOrganizationRemoveRoleReturns(int organizationId, OrganizationRole organizationRole, int userId, Result<OrganizationRight, OperationFailure> result)
        {
            _organizationRightsServiceMock.Setup(x => x.RemoveRole(organizationId, userId, organizationRole)).Returns(result);
        }

        private void ExpectItContractRemoveRoleReturns(ItContract roleObject, int roleId, int userId, Result<ItContractRight, OperationError> result)
        {
            _itContractRightServiceMock.Setup(x => x.RemoveRole(roleObject, roleId, userId)).Returns(result);
        }

        private void ExpectItSystemRemoveRoleReturns(ItSystemUsage roleObject, int roleId, int userId, Result<ItSystemRight, OperationError> result)
        {
            _itSystemRightServiceMock.Setup(x => x.RemoveRole(roleObject, roleId, userId)).Returns(result);
        }

        private void ExpectItProjectRemoveRoleReturns(ItProject roleObject, int roleId, int userId, Result<ItProjectRight, OperationError> result)
        {
            _itProjectRightServiceMock.Setup(x => x.RemoveRole(roleObject, roleId, userId)).Returns(result);
        }

        private void ExpectOrganizationUnitRemoveRoleReturns(OrganizationUnit roleObject, int roleId, int userId, Result<OrganizationUnitRight, OperationError> result)
        {
            _organizationUnitRightServiceMock.Setup(x => x.RemoveRole(roleObject, roleId, userId)).Returns(result);
        }

        private void ExpectDeleteIdentitiesForUserReturns(IEnumerable<SsoUserIdentity> identities)
        {
            _ssoUserIdentityRepository.Setup(x => x.DeleteIdentitiesForUser(identities));
        }

        private User SetupUser(int roleId, OrganizationRole organizationRole)
        {
            var user = new User
            {
                Id = A<int>(),
                DataProcessingRegistrationRights = new List<DataProcessingRegistrationRight>(),
                OrganizationRights = new List<OrganizationRight>(),
                ItContractRights = new List<ItContractRight>(),
                ItSystemRights = new List<ItSystemRight>(),
                ItProjectStatuses = new List<ItProjectStatus>(),
                OrganizationUnitRights = new List<OrganizationUnitRight>(),
                SsoIdentities = new List<SsoUserIdentity>()
            };

            var dprRight = new DataProcessingRegistrationRight
            {
                ObjectId = A<int>(),
                RoleId = roleId
            };
            user.DataProcessingRegistrationRights.Add(dprRight);
            ExpectDprApplicationRemoveRoleReturns(dprRight.ObjectId, roleId, user.Id, Result<DataProcessingRegistrationRight, OperationError>.Success(dprRight));

            var organizationRight = new OrganizationRight
            {
                OrganizationId = A<int>(),
                Role = organizationRole
            };
            user.OrganizationRights.Add(organizationRight);
            ExpectOrganizationRemoveRoleReturns(organizationRight.OrganizationId, organizationRole, user.Id, Result<OrganizationRight, OperationFailure>.Success(organizationRight));

            var itContractRight = new ItContractRight
            {
                Object = new ItContract(),
                RoleId = roleId
            };
            user.ItContractRights.Add(itContractRight);
            ExpectItContractRemoveRoleReturns(itContractRight.Object, roleId, user.Id, Result<ItContractRight, OperationError>.Success(itContractRight));

            var itSystemRight = new ItSystemRight
            {
                Object = new ItSystemUsage(),
                RoleId = roleId
            };
            user.ItSystemRights.Add(itSystemRight);
            ExpectItSystemRemoveRoleReturns(itSystemRight.Object, roleId, user.Id, Result<ItSystemRight, OperationError>.Success(itSystemRight));

            var itProjectRight = new ItProjectRight
            {
                Object = new ItProject(),
                RoleId = roleId
            };
            user.ItProjectRights.Add(itProjectRight);
            ExpectItProjectRemoveRoleReturns(itProjectRight.Object, roleId, user.Id, Result<ItProjectRight, OperationError>.Success(itProjectRight));

            var organizationUnitRight = new OrganizationUnitRight
            {
                Object = new OrganizationUnit(),
                RoleId = roleId
            };
            user.OrganizationUnitRights.Add(organizationUnitRight);
            ExpectOrganizationUnitRemoveRoleReturns(organizationUnitRight.Object, roleId, user.Id, Result<OrganizationUnitRight, OperationError>.Success(organizationUnitRight));

            user.SsoIdentities.Add(new SsoUserIdentity());
            ExpectDeleteIdentitiesForUserReturns(user.SsoIdentities);

            return user;
        }
    }
}
